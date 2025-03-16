using Godot;
using System;

namespace TextConfig
{
    public enum SheetSide
    {
        Right,
        Bottom,
        Left,
        Top,
        Internal
    }

    public enum TextHalf
    {
        Left,
        Right
    }

    public struct TextBlockParams
    {
        public TextBlockParams() {}

        public SheetSide Side = SheetSide.Right;

        public TextHalf Half = TextHalf.Left;

        public String Text = "Lorem ipsum\nto the\nNth1ndegree";

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
}
