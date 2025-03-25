using Godot;
using System;

public partial class Main : Node2D
{
    public static Main Instance
    {
        get;
        private set;
    }

    public override void _Ready()
    {
        Instance = this;

        if (Loader.Instance != null)
        {
            Loader.Instance.OnLoadComplete(this);
        }
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("Back"))
        {
            Loader.Instance.BackToMenu();
        }
    }
}
