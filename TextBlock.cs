using Godot;
using TextConfig;

public partial class TextBlock : RichTextLabel
{
    bool IsLaidout = false;

    TextBlockParams InnerParams;

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

            IsLaidout = true;
        }
    }
}
