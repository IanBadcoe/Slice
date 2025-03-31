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

    bool EnableDrawInner = false;

    public bool EnableDraw
    {
        get { return EnableDrawInner; }
        set
        {
            EnableDrawInner = value;
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
        else if (Params.Half == TextHalf.Both)
        {
            Text = "[fill]" + Params.Text + "[/fill]";
        }
        else
        {
            Text = Params.Text;
        }

        if (Params.Color.HasValue)
        {
            Color fix_alpha = new Color(Params.Color.Value.R, Params.Color.Value.G, Params.Color.Value.B, 1);
            SelfModulate = fix_alpha;
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

        if (Params.Half == TextHalf.Both)
        {
            SnapPoints = new SnapPoint[num_snap_points * 2];
        }
        else
        {
            SnapPoints = new SnapPoint[num_snap_points];
        }

        // if we are the left-half of a text, then we dock something else to the right, and vice-versa
        // or, both...
        switch(Params.Half)
        {
            case TextHalf.Left:
                StoreSnapPoints(Size.X + HalfSpace, 0, num_snap_points);
                break;
            case TextHalf.Right:
                StoreSnapPoints(-HalfSpace, 0, num_snap_points);
                break;
            case TextHalf.Both:
                StoreSnapPoints(Size.X + HalfSpace, 0, num_snap_points);
                StoreSnapPoints(-HalfSpace, num_snap_points, num_snap_points);
                break;
        }
    }

    void StoreSnapPoints(float x_pos, int offset, int num_snap_points)
    {
        float y_start = LineHeight / 2;

        switch(Params.Half)
        {
            case TextHalf.Left:
                x_pos = Size.X + HalfSpace;
                break;

            case TextHalf.Right:
                x_pos = -HalfSpace;
                break;
        }

        for(int i = 0; i < num_snap_points; i++)
        {
            SnapPoints[i + offset] = new SnapPoint(new Vector2(x_pos, y_start + i * LineHeight), 0, i + offset, this);
        }
    }

    public bool AnySnapPointsFor(TextHalf half)
    {
        // we have some points to return if either:
        // - we were asked for boths
        // - we have boths
        // - we were asked for what we have
        return half == TextHalf.Both
            || Params.Half == TextHalf.Both
            || Params.Half == half;
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
    //
    //
    // see comment on AnySnapPointsFor for whether "half" will selected anything in any given case
    public IEnumerable<SnapPoint> GetTransformedSnapPoints(TextHalf half, Transform2D? override_transform = null, Vector2? centre = null)
    {
        if (SnapPoints == null)
        {
            return null;
        }

        int skip = 0;
        int take = SnapPoints.Count();

        // we have either Left or Right or Both sorts of snap-point (Params.Half)
        // and we are asked for either Left or Right or Both (half)
        //
        // if what we are asked for matches what we have, we can just return everything, OR,
        // if we are asked for Both, then everything matches and again we can just return everything
        if (half != Params.Half && half != TextHalf.Both)
        {
            // otherwise...
            // if they don't match, and we don't have "Both" then nothing matches
            if (Params.Half != TextHalf.Both)
            {
                return null;
            }

            // otherwise othersie...
            // we have Both and we were asked for either Left or Right
            // so we return the matching half of our array
            // (we stored them in the order Left, Right)
            take /= 2;
            skip = half == TextHalf.Left ? 0 : take;
        }

        if (override_transform.HasValue)
        {
            Debug.Assert(centre.HasValue);
            Transform2D local_trans = GetTransform();

            return SnapPoints
                .Skip(skip)
                .Take(take)
                .Select(x => override_transform.Value * ((local_trans * x) - centre.Value));
        }
        else
        {
            Debug.Assert(!centre.HasValue);
            return SnapPoints
                .Skip(skip)
                .Take(take)
                .Select(x => GetGlobalTransform() * x);
        }
    }
}
