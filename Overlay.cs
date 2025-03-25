using Godot;
using System;
using System.Linq;
using TextConfig;

public partial class Overlay : Panel
{
    bool EnableOverlay = true;

    Sheet LastFocusSheet;

    public override void _Process(double delta)
    {
        if (Main.Instance == null)
        {
            return;
        }

        if (!EnableOverlay)
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
        if (Main.Instance == null)
        {
            return;
        }

        if (!EnableOverlay || DragDropController.Instance.FocusSheet == null)
        {
            return;
        }

        foreach(Sheet sheet in Main.Instance.GetChildren()
                    .OfType<Sheet>()
                    .Where(x => x != DragDropController.Instance.FocusSheet))
        {
            DrawSheet(sheet);
        }

        DrawSheet(DragDropController.Instance.FocusSheet, DragDropController.Instance.DragTransform);
    }

    private void DrawSheet(Sheet sheet, Transform2D? override_transform = null)
    {
        {
            Vector2 offset2 = new Vector2(-6, 7);
            Vector2 offset3 = new Vector2(-1, 14);

            Color col = /* is_left ? new Color(1,0,0) : */ new Color(0, 1, 0);

            foreach(var sp in sheet.GetTransformedSnapPoints(TextHalf.Left, override_transform))
            {
                // an arrow
                //                (side1)
                //                     \
                //  (back) -------------- (point)
                //                     /
                //                (side2)
                Vector2 side2;
                Vector2 back;

                side2 = sp.Position + offset2.Rotated(sp.Rotation);
                back = sp.Position + offset3.Rotated(sp.Rotation);

                DrawLine(sp.Position, side2, col, 2);
                DrawLine(sp.Position, back, col, 2);
            }
        }

        {
            Vector2 offset1 = new Vector2(6, 7);
            Vector2 offset3 = new Vector2(1, 14);

            Color col = new Color(1,0,0);

            foreach(var sp in sheet.GetTransformedSnapPoints(TextHalf.Right, override_transform))
            {
                // an arrow
                //                (side1)
                //                     \
                //  (back) -------------- (point)
                //                     /
                //                (side2)
                Vector2 side1;
                Vector2 back;

                side1 = sp.Position + offset1.Rotated(sp.Rotation);
                back = sp.Position + offset3.Rotated(sp.Rotation);

                DrawLine(sp.Position, side1, col, 2);
                DrawLine(sp.Position, back, col, 2);
            }
        }
    }
}
