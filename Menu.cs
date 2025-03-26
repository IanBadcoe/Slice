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

    public void OnTest3Pressed()
    {
        Loader.Instance.LoadLevel("res://Test3/SheetSet.json");
    }

    public void OnTest4Pressed()
    {
        Loader.Instance.LoadLevel("res://Test4/SheetSet.json");
    }

    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
