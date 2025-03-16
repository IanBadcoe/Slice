using Godot;
using System;
using System.Collections;
using System.Diagnostics;

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

    Vector2 LastMousePosition;

    // Singletoo Instance
    // vvvvvvvvvvvvvvvvvv

//    static PackedScene DDCScene = GD.Load<PackedScene>("res://DragDropController.tscn");
    static DragDropController Instance;

    public override void _Ready()
    {
        Debug.Assert(Instance == null);

        Instance = this;
    }

    public static DragDropController GetInstance()
    {
        return Instance;
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
        if (DragSheet != null)
        {
            return DragSheet == sheet;
        }

        return MouseFocusSheet == sheet;
    }

    // speed of movement (allowing for us to maybe holding the "fine adjustment" action)
    public float RotationSpeed
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

    public float DragSpeedFactor
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
}