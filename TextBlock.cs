using System.Collections.Generic;
using System.Linq;
using Godot;
using TextConfig;

public partial class TextBlock : RichTextLabel
{
    public const float HalfSpace = 2.0f;       ///< world units, found empirically
    public const float LineHeight = 26.0f;     ///< world units, found empirically

    bool IsLaidout = false;

    TextBlockParams InnerParams;

    bool EnableDebugDrawInner = false;

    public bool EnableDebugDraw
    {
        get { return EnableDebugDrawInner; }
        set
        {
            EnableDebugDrawInner = value;
            QueueRedraw();
        }
    }

    public SnapPoint[] SnapPoints
    {
        get;
        private set;
    }

    public Sheet Sheet
    {
        get;
        set;
    }

    public TextBlockParams Params
    {
        get { return InnerParams; }
        set { InnerParams = value; }
    }

    public override void _Ready()
    {
        var new_sb = new StyleBoxFlat();
        new_sb.BgColor = Color.FromHsv(0.5f, 0.5f, 0.5f);
        AddThemeStyleboxOverride("normal", new_sb);

        if (Params.Half == TextConfig.TextHalf.Left)
        {
            Text = "[right]" + Params.Text + "[/right]";
        }
        else
        {
            Text = Params.Text;
        }
    }

    public override void _Process(double delta)
    {
        if (!IsLaidout)
        {
            Sheet.PostLayout(this, Params);


            StoreSnapPoints();


            IsLaidout = true;
        }
    }

    // SnapPoints
    // vvvvvvvvvv

    void StoreSnapPoints()
    {
        int num_snap_points = (int)(Size.Y / LineHeight);

        SnapPoints = new SnapPoint[num_snap_points];

        float x_pos = 0;
        float y_start = LineHeight / 2;

        switch(Params.Half)
        {
            // if we are the left-half of a text, then we dock something else to the right, and vice-versa
            case TextHalf.Left:
                x_pos = Size.X + HalfSpace;
                break;

            case TextHalf.Right:
                x_pos = -HalfSpace;
                break;
        }

        for(int i = 0; i < num_snap_points; i++)
        {
            SnapPoints[i] = new SnapPoint{
                Point = new Vector2(x_pos, y_start + i * LineHeight),
                Angle = 0                // we're none of us rotated, relative to ourself, but this gets transformed
            };
        }
    }

    public IEnumerable<SnapPoint> GetTransformedSnapPoints()
    {
        if (SnapPoints == null)
        {
            return null;
        }

        return SnapPoints.Select(
            x =>
            {
                return GetGlobalTransform() * x;
            }
        );
    }
}
