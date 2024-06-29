using Godot;

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
}