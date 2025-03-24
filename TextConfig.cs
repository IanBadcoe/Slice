using Godot;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TextConfig
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SheetSide
    {
        Right,
        Bottom,
        Left,
        Top,
        Internal
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TextHalf
    {
        Left,
        Right
    }

    public class TextParams
    {
        public TextParams() {}

        public string Name;

        public SheetSide Side = SheetSide.Right;

        public TextHalf Half = TextHalf.Left;

        public string Text = "Lorem ipsum\nto the\nNth1ndegree";

        public Vector2 Position = new Vector2(0,0);

        public float HalfPosition
        {
            get {
                return Position.X;
            }
            set {
                Position.X = value;
            }
        }

        public float Rotation = 0;      ///< only applies when "internal"
    }

    public class TextSet : Dictionary<string, TextParams>
    {

    }

    public class SheetConfig
    {
        public string Name;

        public Vector2 Size;

        public TextSet Texts;
    }

    public class SheetSet
    {
        public string Name;

        public string[] Sheets;
    }
}
