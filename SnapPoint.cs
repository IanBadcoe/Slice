using System;
using System.Data.Common;
using Godot;

public struct SnapPoint
{
    public SnapPoint(Vector2 position, float rotation, int index, TextBlock text_block)
    {
        Position = position;
        Rotation = rotation;
        TextBlock = text_block;
        IndexInTextBlock = index;
    }

    public Vector2 Position
    {
        get;
        private set;
    }

    public float Rotation
    {
        get;
        private set;
    }

    public TextBlock TextBlock
    {
        get;
        private set;
    }

    public int IndexInTextBlock
    {
        get;
        private set;
    }

    public static SnapPoint operator*(Transform2D left, SnapPoint right)
    {
        return new SnapPoint{
            Position = left * right.Position,
            Rotation = left.Rotation + right.Rotation,
            TextBlock = right.TextBlock,
            IndexInTextBlock = right.IndexInTextBlock
        };
    }

    public static SnapPoint operator*(SnapPoint left, Transform2D right)
    {
        return new SnapPoint{
            Position = left.Position * right,
            Rotation = left.Rotation + right.Rotation,
            TextBlock = left.TextBlock,
            IndexInTextBlock = left.IndexInTextBlock
        };
    }

    public static SnapPoint operator-(SnapPoint left, Vector2 right)
    {
        return new SnapPoint{
            Position = left.Position - right,
            Rotation = left.Rotation,
            TextBlock = left.TextBlock,
            IndexInTextBlock = left.IndexInTextBlock
        };
    }
}