using System.Data.Common;
using Godot;

public struct SnapPoint
{
    public Vector2 Point;

    public float Angle;

    public static SnapPoint operator*(Transform2D left, SnapPoint right)
    {
        return new SnapPoint{
            Point = left * right.Point,
            Angle = left.Rotation / Mathf.Pi * 180 + right.Angle
        };
    }
}