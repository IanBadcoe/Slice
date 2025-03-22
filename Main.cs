using Godot;
using System;
using System.Linq;

public partial class Main : Node2D
{
    bool Initialised = false;

    public override void _Process(double delta)
    {
        if (!Initialised)
        {
            DragDropController.Instance.Main = this;
            GetChildren().OfType<DebugOverlay>().First().Main = this;

            Initialised = true;
        }
    }
}
