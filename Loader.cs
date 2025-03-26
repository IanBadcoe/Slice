using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Godot;
using GodotPlugins.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TextConfig;

public partial class Loader : Node
{
    public static Loader Instance
    {
        get;
        private set;
    }

    string JsonResourceBeingLoaded;

    static PackedScene SheetScene = GD.Load<PackedScene>("res://Sheet.tscn");

    public Loader()
    {
        Instance = this;
    }

    private SheetConfig LoadSheetConfigViaJson(string resource_path)
    {
        FileAccess file = FileAccess.Open(resource_path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();

        SheetConfig sheet_config = JsonConvert.DeserializeObject<SheetConfig>(json);

        foreach(var entry in sheet_config.Texts)
        {
            entry.Value.Name = entry.Key;
        }

        return sheet_config;
    }

    private Sheet LoadSheetViaJson(string resource_path, Node2D add_to)
    {
        SheetConfig config = LoadSheetConfigViaJson(resource_path);

        Sheet sheet = SheetScene.Instantiate<Sheet>();

        add_to.AddChild(sheet);

        foreach(var entry in config.Texts)
        {
            sheet.AddTextBlock(entry.Value);
        }

        sheet.Size = config.Size;

        if (!String.IsNullOrEmpty(config.Texture))
        {
            string resource_stem = resource_path.Left(resource_path.LastIndexOf('/') + 1);

            if (!config.Texture.StartsWith("res:"))
            {
                sheet.TextureResourcePath = resource_stem + config.Texture;
            }
            else
            {
                sheet.TextureResourcePath = config.Texture;
            }
        }

        return sheet;
    }

    private void LoadSheetSetViaJson(string resource_path, Main main)
    {
        FileAccess file = FileAccess.Open(resource_path, FileAccess.ModeFlags.Read);

        string json = file.GetAsText();

        SheetSet set = JsonConvert.DeserializeObject<SheetSet>(json);

        string resource_stem = resource_path.Left(resource_path.LastIndexOf('/') + 1);

        foreach (string sheet_resource in set.Sheets)
        {
            if(!sheet_resource.StartsWith("res:", StringComparison.CurrentCultureIgnoreCase))
            {
                LoadSheetViaJson(resource_stem + sheet_resource, main);
            }
            else
            {
                LoadSheetViaJson(sheet_resource, main);
            }
        }
    }

    public void LoadLevel(string resource_path)
    {
        JsonResourceBeingLoaded = resource_path;

        GetTree().ChangeSceneToFile("res://Main.tscn");
    }

    public void OnLoadComplete(Main main)
    {
        if (!String.IsNullOrEmpty(JsonResourceBeingLoaded))
        {
            LoadSheetSetViaJson(JsonResourceBeingLoaded, main);

            JsonResourceBeingLoaded = null;
        }
    }

    public void BackToMenu()
    {
        GetTree().ChangeSceneToFile("res://Menu.tscn");
    }
}