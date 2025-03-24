using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using TextConfig;

public partial class TextBlock : RichTextLabel
{
    public const float HalfSpace = 2.0f;       ///< world units, found empirically
    public const float LineHeight = 26.0f;     ///< world units, found empirically

    bool IsLaidout = false;

    TextParams InnerParams;

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

    public TextParams Params
    {
        get { return InnerParams; }
        set { InnerParams = value; }
    }

    public override void _Ready()
    {
        // var new_sb = new StyleBoxFlat();
        // new_sb.BgColor = Color.FromHsv(0.5f, 0.5f, 0.5f);
        // AddThemeStyleboxOverride("normal", new_sb);

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
            SnapPoints[i] = new SnapPoint(
                new Vector2(x_pos, y_start + i * LineHeight),
                0,
                i,
                this
            );
        }
    }

    // If override_transform is given, so must centre and vide-versa
    //
    // without those, just position our snap-points relative to our parent, as normal
    //
    // with them, replace the parent-transform with override_transform
    // e.g. calculates the snap-point position as if the sheet we were in was located
    // at override_transform, rather than it's real current position
    //
    // so what we need to do is first apply our local transform, and then the override
    //
    // and in this case, because our pivot_offset is in the centre, the override_transform will
    // have that added in, so we need to subtract it off here...
    public IEnumerable<SnapPoint> GetTransformedSnapPoints(Transform2D? override_transform = null, Vector2? centre = null)
    {
        if (SnapPoints == null)
        {
            return null;
        }

        if (override_transform.HasValue)
        {
            Debug.Assert(centre.HasValue);
            Transform2D local_trans = GetTransform();

            return SnapPoints.Select(
                x => override_transform.Value * ((local_trans * x) - centre.Value)
            );
        }
        else
        {
            Debug.Assert(!centre.HasValue);
            return SnapPoints.Select(
                x =>
                {
                    return GetGlobalTransform() * x;
                }
            );
        }
    }
}
