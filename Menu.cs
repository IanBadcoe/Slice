using Godot;
using System;

public partial class Menu : Control
{
    public void OnTest1Pressed()
    {
        Loader.Instance.LoadLevel("res://Test1/SheetSet.json");
    }

    public void OnTest2Pressed()
    {
        Loader.Instance.LoadLevel("res://Test2/SheetSet.json");
    }

    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
