using Godot;
using System.Diagnostics;
using TextConfig;
using System.Linq;
using System;
using System.Collections.Generic;

public partial class DragDropController : Node2D
{
    const float RotationSpeedConst = 100.0f;    ///< degress/s
    const float SnapDistance = 20f;             ///< screen units

    public Main Main;

    enum DragMode
    {
        None,
        Dragging,
        RotateDragging
    }

    Sheet MouseFocusSheet;

    DragMode Mode = DragMode.None;
    Sheet DragSheet;

    // we need non-drag rotation to pick up the "transaction-like" behaviour of a drag
    // so that snapping will be able to track its "real" position and its "snapped" position
    bool SawANonDragRotate = false;
    bool SawANonDragRotateThisFrame = false;

    Vector2 LastMousePosition;

    SnapPoint[] StaticLSnaps;
    SnapPoint[] StaticRSnaps;

    public Vector2 DragPosition
    {
        get;
        private set;
    }

    public float DragRotation
    {
        get;
        private set;
    }

    public Transform2D DragTransform
    {
        get
        {
            return new Transform2D(DragRotation, DragPosition);
        }
    }

    // Singleton Instance
    // vvvvvvvvvvvvvvvvvv

    public static DragDropController Instance
    {
        get;
        private set;
    }

    DragDropController()
    {
        Debug.Assert(Instance == null);

        Instance = this;
    }

    // Input
    // vvvvv

    public override void _Input(InputEvent @event)
    {
        switch(Mode)
        {
            case DragMode.None:
                break;

            case DragMode.Dragging:
                HandleInput_Dragging(@event);
                break;

            case DragMode.RotateDragging:
                HandleInput_RotateDragging(@event);
                break;
        }
    }

    void HandleInput_Dragging(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouse_motion)
        {
            Vector2 delta = mouse_motion.Position - LastMousePosition;
            DragPosition += delta * DragSpeedFactor;
            LastMousePosition = mouse_motion.Position;

            HandleSnapping();
        }
    }

    void HandleInput_RotateDragging(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouse_motion)
        {
            float delta = mouse_motion.Position.Y - LastMousePosition.Y;
            DragRotation += delta * DragSpeedFactor / 180 * MathF.PI;
            LastMousePosition = mouse_motion.Position;

            HandleSnapping();
        }
    }

    // Sheets gaining and losing mouse focus
    //
    // - when the pointer moves over one, it gains focus
    // - when the pointer leaves one, if loses focus
    // - EXCEPT:
    // -- if we are dragging, the dragged sheet hangs on to the focus until dropped
    // -- this involves having two members:
    // --- MouseFocusSheet <-- the one that really has the pointer over it
    // --- DragSheet <-- the one we are dragging
    // -- and when other classes query whether they have focus, the latter overrides the former, if it is set

    public void TryGetMouseFocus(Sheet sheet)
    {
        if (MouseFocusSheet == sheet)
        {
            return;
        }

        MouseFocusSheet = sheet;

        if (DoIHaveFocus(sheet))
        {
            Debug.Print("Set DragPosition");
            DragPosition = sheet.Centre;
            DragRotation = sheet.Rotation;
        }
    }

    public void LoseMouseFocus(Sheet sheet)
    {
        if (MouseFocusSheet == sheet)
        {
            MouseFocusSheet = null;
        }
    }

    public bool DoIHaveFocus(Sheet sheet)
    {
        return FocusSheet == sheet;
    }

    public Sheet FocusSheet
    {
        get
        {
            return DragSheet ?? MouseFocusSheet;
        }
    }

    // speed of movement (allowing for us to maybe holding the "fine adjustment" action)
    float RotationSpeed
    {
        get
        {
            if (Input.IsActionPressed("FineAdjust"))
            {
                return RotationSpeedConst / 10;
            }

            return RotationSpeedConst;
        }
    }

    float DragSpeedFactor
    {
        get
        {
            if (Input.IsActionPressed("FineAdjust"))
            {
                return 0.1f;
            }

            return 1f;
        }
    }

    // Sheets starting and stopping drags
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

    public void StartDragging(Sheet sheet)
    {
        if (Mode == DragMode.None && MouseFocusSheet == sheet)
        {
            Mode = DragMode.Dragging;
            DragSheet = MouseFocusSheet;

            LastMousePosition = GetViewport().GetMousePosition();

            BeginSnapping();
        }
    }

    public void EndDragging(Sheet sheet)
    {
        if (DragSheet == sheet)
        {
            Mode = DragMode.None;
            DragSheet = null;

            EndSnapping();
        }
    }

    public void StartRotateDragging(Sheet sheet)
    {
        if (Mode == DragMode.None && sheet == MouseFocusSheet)
        {
            Mode = DragMode.RotateDragging;
            DragSheet = MouseFocusSheet;

            LastMousePosition = GetViewport().GetMousePosition();

            BeginSnapping();
        }
    }

    public void EndRotateDragging(Sheet sheet)
    {
        if (DragSheet == sheet)
        {
            Mode = DragMode.None;
            DragSheet = null;

            EndSnapping();
        }
    }

    // handling (non drag) rotate actions
    // (allowing for snapping)
    // vvvvvvvvvvvvvvvvvvvvvvv
    public void RotateSheet(float delta)
    {
        DragRotation += delta * DragSpeedFactor / 180 * MathF.PI * 50;

        if (!SawANonDragRotate)
        {
            BeginSnapping();
        }

        SawANonDragRotateThisFrame = true;
        SawANonDragRotate = true;

        HandleSnapping();
    }

    public override void _Process(double delta)
    {
        if (Main == null)
        {
            Main = GetNodeOrNull<Main>("/root/Main");
        }

        if (Main == null)
        {
            return;
        }

        if (SawANonDragRotate)
        {
            if (!SawANonDragRotateThisFrame)
            {
                EndSnapping();
            }

            SawANonDragRotateThisFrame = false;
        }
    }

    // snapping
    // vvvvvvvv
    private void BeginSnapping()
    {
        if (Main == null)
        {
            return;
        }

        StaticRSnaps = Main.GetChildren()
            .OfType<Sheet>()
            .Where(x => x != FocusSheet)
            .Select(x => x.GetTransformedSnapPoints(TextHalf.Right))
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();

        StaticLSnaps = Main.GetChildren()
            .OfType<Sheet>()
            .Where(x => x != FocusSheet)
            .Select(x => x.GetTransformedSnapPoints(TextHalf.Left))
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();
    }

    private void HandleSnapping()
    {
        IEnumerable<SnapPoint> MovingLSnaps = FocusSheet.GetTransformedSnapPoints(TextHalf.Left, DragTransform);
        IEnumerable<SnapPoint> MovingRSnaps = FocusSheet.GetTransformedSnapPoints(TextHalf.Right, DragTransform);

        if (MovingLSnaps != null && StaticRSnaps != null)
        {
            foreach(SnapPoint mov in MovingLSnaps)
            {
                foreach(SnapPoint stat in StaticRSnaps)
                {
                    if (Mathf.Abs(mov.Position.X - stat.Position.X) < SnapDistance
                        && Mathf.Abs(mov.Position.Y - stat.Position.Y) < SnapDistance)
                    {
                        BringSheetToSnapPoint(stat, FocusSheet, mov.TextBlock, mov.TextBlock.SnapPoints[mov.IndexInTextBlock]);

                        return;
                    }
                }
            }
        }

        if (MovingRSnaps != null && StaticLSnaps != null)
        {
            foreach(SnapPoint mov in MovingRSnaps)
            {
                foreach(SnapPoint stat in StaticLSnaps)
                {
                    if (Mathf.Abs(mov.Position.X - stat.Position.X) < SnapDistance
                        && Mathf.Abs(mov.Position.Y - stat.Position.Y) < SnapDistance)
                    {
                        BringSheetToSnapPoint(stat, FocusSheet, mov.TextBlock, mov.TextBlock.SnapPoints[mov.IndexInTextBlock]);

                        return;
                    }
                }
            }
        }

        Vector2 pivot_offset = FocusSheet.PivotOffset;

        FocusSheet.Position = DragPosition - pivot_offset;

        FocusSheet.PivotOffset = pivot_offset;

        FocusSheet.Rotation = DragTransform.Rotation;
    }

    private void BringSheetToSnapPoint(SnapPoint still_snap_point, Sheet moving_s, TextBlock moving_tb, SnapPoint moving_sp)
    {
        SnapPoint moving_sp_world_pos_pre_rotate = moving_tb.GetGlobalTransform() * moving_sp;

        // rotate the sheet by the difference between the current rotation of the moving sp
        // and the rotation of the still sp that it wants to have
        moving_s.Rotation += still_snap_point.Rotation - moving_sp_world_pos_pre_rotate.Rotation;

        // fixing the rotation can move the snap-point
        SnapPoint moving_sp_world_pos_post_rotate = moving_tb.GetGlobalTransform() * moving_sp;

        // move the moving-sheet by the error between where its current position puts the moving_sp
        // and the position of the still sp where it wants to be
        moving_s.Position += still_snap_point.Position - moving_sp_world_pos_post_rotate.Position;

        // don't, as it makes it impossible to back away from an accidental snap
        // // move the dran snap-points to the new position
        // DragPosition = moving_s.Centre;
        // DragRotation = moving_s.Rotation;
    }

    private void EndSnapping()
    {
        SawANonDragRotate = false;

        Debug.Print("EndSnapping");
    }
}