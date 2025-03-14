using Godot;
using System;

public partial class Sheet : Panel
{
    static PackedScene TextBlockScene = GD.Load<PackedScene>("res://TextBlock.tscn");
    const float HalfSpace = 2.0f;

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

    public void AddTextBlock(TextBlockParams Params)
    {
        var new_tb = TextBlockScene.Instantiate<TextBlock>();

        new_tb.Params = Params;
        new_tb.Sheet = this;

        AddChild(new_tb);
    }

    public void PostLayout(TextBlock TextBlock, TextBlockParams Params)
    {
        TextBlock.Position = Vector2.Zero;
        TextBlock.RotationDegrees = 0;
        //TextBlock.PivotOffset

        int rot_in_right_angles = 0;

        switch(Params.Side)
        {
            case SheetSide.Right:
                TextBlock.Position = new Vector2(Size.X - TextBlock.Size.X - HalfSpace, Params.HalfPosition);
                break;

            case SheetSide.Left:
                TextBlock.Position = new Vector2(HalfSpace, Size.Y - Params.HalfPosition);
                rot_in_right_angles = 2;
                break;

            case SheetSide.Top:
                TextBlock.Position = new Vector2(Params.HalfPosition, HalfSpace);
                rot_in_right_angles = 3;
                break;

            case SheetSide.Bottom:
                TextBlock.Position = new Vector2(Size.X - Params.HalfPosition, Size.Y - TextBlock.Size.X - HalfSpace);
                rot_in_right_angles = 1;
                break;

            case SheetSide.Internal:
                TextBlock.Position = Params.Position;
                break;
        }

        if (Params.Side != SheetSide.Internal)
        {
            if (Params.Half == TextHalf.Right)
            {
                rot_in_right_angles += 2;
            }

            rot_in_right_angles %= 4;

            TextBlock.RotationDegrees = rot_in_right_angles * 90;

            switch(rot_in_right_angles)
            {
                case 0:
                    break;
                case 1:
                    TextBlock.Position += new Vector2(TextBlock.Size.Y, 0);
                    break;
                case 2:
                    TextBlock.Position += TextBlock.Size;
                    break;
                case 3:
                    TextBlock.Position += new Vector2(0, TextBlock.Size.X);
                    break;
            }
        }
        else
        {
            float sk = MathF.Sin(Params.Rotation / 360 * 2 * MathF.PI);
            float ck = MathF.Cos(Params.Rotation / 360 * 2 * MathF.PI);

            float dx = 0;
            float dy = -TextBlock.Size.Y / 2;

            if (Params.Half == TextHalf.Left)
            {
                dx = - HalfSpace - TextBlock.Size.X;
            }
            else
            {
                dx = HalfSpace;
            }

            TextBlock.RotationDegrees = Params.Rotation;

            TextBlock.Position += new Vector2(
                dx * ck - dy * sk, dx * sk + dy * ck
            );
        }
    }

    public override void _Ready()
    {
        var new_sb = new StyleBoxFlat();
        new_sb.BgColor = Color.FromHsv((float)Random.Shared.NextDouble(), 0.5f, 0.5f);
        AddThemeStyleboxOverride("panel", new_sb);

        {
            var Params = new TextBlockParams
            {
                Text = "1. [b]left[/b]\n[i]left[/i]",
                Side = SheetSide.Right,
                Half = TextHalf.Left,
                HalfPosition = 100
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "[b][i]right[/i][/b]\nright .1",
                Side = SheetSide.Left,
                Half = TextHalf.Right,
                HalfPosition = 400
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "2. left\nleft",
                Side = SheetSide.Top,
                Half = TextHalf.Left,
                HalfPosition = 100
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "right\nright .2",
                Side = SheetSide.Bottom,
                Half = TextHalf.Right,
                HalfPosition = 400
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "right\nright .3",
                Side = SheetSide.Right,
                Half = TextHalf.Right,
                HalfPosition = 400
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "3. left\nleft",
                Side = SheetSide.Left,
                Half = TextHalf.Left,
                HalfPosition = 100
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "right\nright .4",
                Side = SheetSide.Top,
                Half = TextHalf.Right,
                HalfPosition = 400
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "4. left\nleft",
                Side = SheetSide.Bottom,
                Half = TextHalf.Left,
                HalfPosition = 100
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "right\nright .5",
                Side = SheetSide.Internal,
                Half = TextHalf.Right,
                Position = new Vector2(250, 250),
                Rotation = 135
            };

            AddTextBlock(Params);
        }

        {
            var Params = new TextBlockParams
            {
                Text = "5. left\nleft",
                Side = SheetSide.Internal,
                Half = TextHalf.Left,
                Position = new Vector2(250, 250),
                Rotation = 135
            };

            AddTextBlock(Params);
        }
    }
}
