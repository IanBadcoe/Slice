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

    public Vector2[] SnapPoints
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

        SnapPoints = new Vector2[num_snap_points];

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
            SnapPoints[i] = new Vector2(x_pos, y_start + i * LineHeight);
        }
    }

    public Vector2[] GetTransformedSnapPoints()
    {
        return SnapPoints.Select(x =>
        {
            return GetGlobalTransform() * x;
        }
        ).ToArray();
    }

    // debug draw
    // vvvvvvvvvv
    public override void _Draw()
    {
        if (!EnableDebugDraw || SnapPoints == null)
        {
            return;
        }

        bool is_left = Params.Half == TextHalf.Left;

        Vector2 offset1 = new Vector2(-7, 5);
        Vector2 offset2 = new Vector2(-7, -5);
        Vector2 offset3 = new Vector2(-14, 0);

        Color col =  is_left ? new Color(1,0,0) : new Color(0, 1, 0);

        foreach(var point in SnapPoints)
        {
            // an arrow
            //                (side1)
            //                     \
            //  (back) -------------- (point)
            //                     /
            //                (side2)
            Vector2 side1;
            Vector2 side2;
            Vector2 back;

            if (is_left)
            {
                side1 = point + offset1;
                side2 = point + offset2;
                back = point + offset3;
            }
            else
            {
                side1 = point - offset1;
                side2 = point - offset2;
                back = point - offset3;
            }

            DrawLine(point, side1, col, 4);
            DrawLine(point, side2, col, 4);
            DrawLine(point, back, col, 4);
        }
    }
}
