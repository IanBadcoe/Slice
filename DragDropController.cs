using Godot;
using System.Diagnostics;
using TextConfig;

public partial class DragDropController : Node2D
{
    const float RotationSpeedConst = 100.0f;   ///< degress/s

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
            DragSheet.Position += delta * DragSpeedFactor;
            LastMousePosition = mouse_motion.Position;
        }
    }

    void HandleInput_RotateDragging(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouse_motion)
        {
            float delta = mouse_motion.Position.Y - LastMousePosition.Y;
            DragSheet.RotationDegrees += delta * DragSpeedFactor;
            LastMousePosition = mouse_motion.Position;
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

    public bool TryGetMouseFocus(Sheet sheet)
    {
        MouseFocusSheet = sheet;

        return DoIHaveFocus(sheet);
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
        }
    }

    public void EndDragging(Sheet sheet)
    {
        if (DragSheet == sheet)
        {
            Mode = DragMode.None;
            DragSheet = null;
        }
    }

    public void StartRotateDragging(Sheet sheet)
    {
        if (Mode == DragMode.None && sheet == MouseFocusSheet)
        {
            Mode = DragMode.RotateDragging;
            DragSheet = MouseFocusSheet;

            LastMousePosition = GetViewport().GetMousePosition();
        }
    }

    public void EndRotateDragging(Sheet sheet)
    {
        if (DragSheet == sheet)
        {
            Mode = DragMode.None;
            DragSheet = null;
        }
    }

    // handling (non drag) rotate actions
    // (allowing for snapping)
    // vvvvvvvvvvvvvvvvvvvvvvv
    public void RotateSheet(float delta)
    {
        FocusSheet.RotationDegrees += RotationSpeed * (float)delta;

        if (!SawANonDragRotate)
        {
            BeginSnapping();
        }

        SawANonDragRotateThisFrame = true;
        SawANonDragRotate = true;
    }

    public override void _Process(double delta)
    {
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
        Debug.Print("BeginSnapping");
    }

    private void EndSnapping()
    {
        SawANonDragRotate = false;

        Debug.Print("EndSnapping");
    }
}