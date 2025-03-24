using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        if (Loader.Instance != null)
        {
            Loader.Instance.OnLoadComplete(this);
        }
    }
}
