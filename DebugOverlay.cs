using Godot;
using System;
using TextConfig;

public partial class DebugOverlay : Panel
{
    bool EnableDebugDraw = true;

    Sheet LastFocusSheet;

    public override void _Process(double delta)
    {
        if (!EnableDebugDraw)
        {
            return;
        }

        if (LastFocusSheet != null || LastFocusSheet != DragDropController.Instance.FocusSheet)
        {
            QueueRedraw();

            LastFocusSheet = DragDropController.Instance.FocusSheet;
        }
    }

    // debug draw
    // vvvvvvvvvv
    public override void _Draw()
    {
        if (!EnableDebugDraw || DragDropController.Instance.FocusSheet == null)
        {
            return;
        }

        {
            Vector2 offset2 = new Vector2(-6, 7);
            Vector2 offset3 = new Vector2(-1, 14);

            Color col = /* is_left ? new Color(1,0,0) : */ new Color(0, 1, 0);

            foreach(var sp in DragDropController.Instance.FocusSheet.GetTransformedSnapPoints(TextHalf.Left))
            {
                // an arrow
                //                (side1)
                //                     \
                //  (back) -------------- (point)
                //                     /
                //                (side2)
                Vector2 side2;
                Vector2 back;

                side2 = sp.Point + offset2.Rotated(sp.Angle / 180 * Mathf.Pi);
                back = sp.Point + offset3.Rotated(sp.Angle / 180 * Mathf.Pi);

                DrawLine(sp.Point, side2, col, 2);
                DrawLine(sp.Point, back, col, 2);
            }
        }

        {
            Vector2 offset1 = new Vector2(6, 7);
            Vector2 offset3 = new Vector2(1, 14);

            Color col = new Color(1,0,0);

            foreach(var sp in DragDropController.Instance.FocusSheet.GetTransformedSnapPoints(TextHalf.Right))
            {
                // an arrow
                //                (side1)
                //                     \
                //  (back) -------------- (point)
                //                     /
                //                (side2)
                Vector2 side1;
                Vector2 back;

                side1 = sp.Point + offset1.Rotated(sp.Angle / 180 * Mathf.Pi);
                back = sp.Point + offset3.Rotated(sp.Angle / 180 * Mathf.Pi);

                DrawLine(sp.Point, side1, col, 2);
                DrawLine(sp.Point, back, col, 2);
            }
        }
    }
}
