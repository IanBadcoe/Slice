using Godot;
using System;
using System.Linq;
using TextConfig;

public partial class Sheet : Panel
{
    static PackedScene TextBlockScene = GD.Load<PackedScene>("res://TextBlock.tscn");

    StyleBoxFlat StyleBox;

    DragDropController DDC;

    public Vector2 Centre => GetGlobalTransform() * PivotOffset;

    // adding half texts
    // vvvvvvvvvvvvvvvvv

    public void AddTextBlock(TextParams Params)
    {
        var new_tb = TextBlockScene.Instantiate<TextBlock>();

        new_tb.Params = Params;
        new_tb.Sheet = this;

        AddChild(new_tb);
    }

    public void PostLayout(TextBlock text_block, TextParams @params)
    {
        int rot_in_right_angles = 0;

        switch(@params.Side)
        {
            case SheetSide.Right:
                text_block.Position = new Vector2(Size.X - text_block.Size.X - TextBlock.HalfSpace, @params.HalfPosition);
                break;

            case SheetSide.Left:
                text_block.Position = new Vector2(TextBlock.HalfSpace, Size.Y - @params.HalfPosition);
                rot_in_right_angles = 2;
                break;

            case SheetSide.Top:
                text_block.Position = new Vector2(@params.HalfPosition, TextBlock.HalfSpace);
                rot_in_right_angles = 3;
                break;

            case SheetSide.Bottom:
                text_block.Position = new Vector2(Size.X - @params.HalfPosition, Size.Y - text_block.Size.X - TextBlock.HalfSpace);
                rot_in_right_angles = 1;
                break;

            case SheetSide.Internal:
                text_block.Position = @params.Position;
                break;

            case SheetSide.Span:
                text_block.Position = new Vector2(TextBlock.HalfSpace, Size.Y - @params.HalfPosition);
                // we have fit to this once, giving us the right height
                // but now we override the width so we span the sheet
                text_block.FitContent = false;
                text_block.Size = new Vector2(Size.X - TextBlock.HalfSpace * 2, text_block.Size.Y);
                // rotate as if for the left
                rot_in_right_angles = 2;
                break;
        }

        if (@params.Side != SheetSide.Internal)
        {
            if (@params.Half == TextHalf.Right
                || @params.Half == TextHalf.Both)
            {
                rot_in_right_angles += 2;
            }

            rot_in_right_angles %= 4;

            text_block.RotationDegrees = rot_in_right_angles * 90;

            switch(rot_in_right_angles)
            {
                case 0:
                    break;
                case 1:
                    text_block.Position += new Vector2(text_block.Size.Y, 0);
                    break;
                case 2:
                    text_block.Position += text_block.Size;
                    break;
                case 3:
                    text_block.Position += new Vector2(0, text_block.Size.X);
                    break;
            }
        }
        else
        {
            float sk = MathF.Sin(@params.Rotation / 360 * 2 * MathF.PI);
            float ck = MathF.Cos(@params.Rotation / 360 * 2 * MathF.PI);

            float dx = 0;
            float dy = -text_block.Size.Y / 2;

            if (@params.Half == TextHalf.Left)
            {
                dx = - TextBlock.HalfSpace - text_block.Size.X;
            }
            else
            {
                dx = TextBlock.HalfSpace;
            }

            text_block.RotationDegrees = @params.Rotation;

            text_block.Position += new Vector2(
                dx * ck - dy * sk, dx * sk + dy * ck
            );
        }
    }

    // drag and drop
    // vvvvvvvvvvvvv

    public override void _Process(double delta)
    {
        // debug
        // vvvvv
        foreach(TextBlock tb in GetChildren().OfType<TextBlock>())
        {
            tb.EnableDebugDraw = DDC.DoIHaveFocus(this);
        }

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

    // SnapPoints
    // vvvvvvvvvv

    public SnapPoint[] GetTransformedSnapPoints(TextHalf half, Transform2D? override_transform = null)
    {
        Vector2? pivot = override_transform.HasValue ? PivotOffset : null;

        return GetChildren()
            .OfType<TextBlock>()
            .Where(x => x.AnySnapPointsFor(half))
            .Select(x => x.GetTransformedSnapPoints(half, override_transform, pivot))
            .Where(x => x != null)
            .SelectMany(x => x)
            .ToArray();
    }

    // setup
    // vvvvv

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

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "xxx",//"1. [b]left[/b]\n[i]left[/i]",
        //         Side = SheetSide.Right,
        //         Half = TextHalf.Left,
        //         HalfPosition = 100
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "[b][i]right[/i][/b]\nright .1",
        //         Side = SheetSide.Left,
        //         Half = TextHalf.Right,
        //         HalfPosition = 400
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "2. [u]left[/u]",
        //         Side = SheetSide.Top,
        //         Half = TextHalf.Left,
        //         HalfPosition = 100
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "right\nright .2\nright",
        //         Side = SheetSide.Bottom,
        //         Half = TextHalf.Right,
        //         HalfPosition = 400
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "right\nright .3",
        //         Side = SheetSide.Right,
        //         Half = TextHalf.Right,
        //         HalfPosition = 400
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "3. [color=aqua]left[/color]\n[color=red]left[/color]",
        //         Side = SheetSide.Left,
        //         Half = TextHalf.Left,
        //         HalfPosition = 100
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "right\nright .4",
        //         Side = SheetSide.Top,
        //         Half = TextHalf.Right,
        //         HalfPosition = 400
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "4. left\nleft",
        //         Side = SheetSide.Bottom,
        //         Half = TextHalf.Left,
        //         HalfPosition = 100
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "right\nright .5",
        //         Side = SheetSide.Internal,
        //         Half = TextHalf.Right,
        //         Position = new Vector2(250, 250),
        //         Rotation = 135
        //     };

        //     AddTextBlock(Params);
        // }

        // {
        //     var Params = new TextParams
        //     {
        //         Text = "5. left\nleft",
        //         Side = SheetSide.Internal,
        //         Half = TextHalf.Left,
        //         Position = new Vector2(250, 250),
        //         Rotation = 135
        //     };

        //     AddTextBlock(Params);
        // }
    }
}
