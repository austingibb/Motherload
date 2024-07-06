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
}