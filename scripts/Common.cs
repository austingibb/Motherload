using System.Collections.Generic;
using Godot;

public delegate bool MoneyAuthorization(int cost);

public partial class Common
{
    public static bool ValidTile(Tile tile)
    {
        return tile != null && GodotObject.IsInstanceValid(tile);
    }

    public static float LineOverlap(float left1, float right1, float left2, float right2)
    {
        return Mathf.Max(0, Mathf.Min(right1, right2) - Mathf.Max(left1, left2));
    }

    public static List<Node> GetAllChildren(Node node)
    {
        List<Node> children = new List<Node>();
        foreach (Node child in node.GetChildren())
        {
            children.Add(child);
            children.AddRange(GetAllChildren(child));
        }
        return children;
    }

    public static void GetAngleDiff(float angle1, float angle2, out float direction, out float angleDiff)
    {
        angleDiff = angle2 - angle1;
        direction = Mathf.Sign(angleDiff);
        angleDiff = Mathf.Abs(angleDiff);

        if (angleDiff > Mathf.Pi)
        {
            angleDiff = Mathf.Tau - angleDiff;
            direction *= -1;
        }
    }

    public static float FlipAngleYAxis(float angle)
    {
        return (Mathf.Pi - Mathf.Abs(angle)) * Mathf.Sign(angle);
    }
}