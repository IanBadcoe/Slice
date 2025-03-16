using Godot;
using System;
using TextConfig;

public partial class Sheet : Panel
{
    const float HalfSpace = 2.0f;       ///< pixels

    static PackedScene TextBlockScene = GD.Load<PackedScene>("res://TextBlock.tscn");

    StyleBoxFlat StyleBox;

    DragDropController DDC;

    // adding half texts
    // vvvvvvvvvvvvvvvvv

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

    // drag and drop
    // vvvvvvvvvvvvv

    public override void _Process(double delta)
    {
        // DDC will ignore us if we aren't the sheet with mouse focus
        if (Input.IsActionJustPressed("Grab"))
        {
            DDC.StartDragging(this);
        }

        if (Input.IsActionJustPressed("RotateDrag"))
        {
            DDC.StartRotateDragging(this);
        }

        // we need to correctly end a drag, even if the mouse isn't still over us (e.g. we moved behind something else)
        //
        // DragDropController will ignore us if we aren't being dragged
        if (Input.IsActionJustReleased("Grab"))
        {
            DDC.EndDragging(this);
        }

        if (Input.IsActionJustReleased("RotateDrag"))
        {
            DDC.EndRotateDragging(this);
        }

        if (DDC.DoIHaveFocus(this))
        {
            if (Input.IsActionPressed("RotateCW") || Input.IsActionJustReleased("RotateCW"))
            {
                DDC.RotateSheet((float)delta);
            }

            if (Input.IsActionPressed("RotateCCW") || Input.IsActionJustReleased("RotateCCW"))
            {
                DDC.RotateSheet(-(float)delta);
            }

            ShowBorder(true);
        }
        else
        {
            ShowBorder(false);
        }
    }

    void ShowBorder(bool show)
    {
        int width = 0;

        if (show)
        {
            width = 1;
        }

        StyleBox.BorderWidthLeft = StyleBox.BorderWidthRight = StyleBox.BorderWidthTop = StyleBox.BorderWidthBottom = width;
    }

    public override void _Ready()
    {
        DDC = DragDropController.Instance;

        StyleBox = GetThemeStylebox("panel").Duplicate(false) as StyleBoxFlat;
        AddThemeStyleboxOverride("panel", StyleBox);

        MouseEntered += () =>
        {
            DDC.TryGetMouseFocus(this);
        };
        MouseExited += () =>
        {
            // will ignore us if we don't have it, which we may not, if a drag in progress has
            // prevented us from getting it
            DDC.LoseMouseFocus(this);
        };

        PivotOffset = Size / 2;

        // test code
        // vvvvvvvvv
        StyleBox.BgColor = Color.FromHsv((float)Random.Shared.NextDouble(), 0.5f, 0.5f);

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
