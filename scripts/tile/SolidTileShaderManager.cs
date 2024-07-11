using Godot;

public enum SolidTileSurface
{
    Top,
    Bottom,
    Left,
    Right
}

public enum SolidTileCorner 
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    None
}

public enum SolidTileCornerShape
{
    Straight,
    Convex,
    Concave,
    None
}

public partial class SolidTileShaderManager : GodotObject
{
    public static void SetupAnimation(Node2D drillable, DrillFromDirection drillFromDirection) 
    {
        AnimatedSprite2D animatedSprite2D = drillable.GetNode<AnimatedSprite2D>("Dirt_AnimatedSprite2D");
        ShaderMaterial shaderMaterial = animatedSprite2D.Material as ShaderMaterial;

        if (drillFromDirection == DrillFromDirection.LEFT)
        {
            shaderMaterial.SetShaderParameter("dig_orientation", true);
            shaderMaterial.SetShaderParameter("flip_dig_animation_h", true);
            shaderMaterial.SetShaderParameter("dig_offset", 33);
        } 
        else if (drillFromDirection == DrillFromDirection.RIGHT) 
        {
            shaderMaterial.SetShaderParameter("dig_orientation", true);
            shaderMaterial.SetShaderParameter("flip_dig_animation_h", false);
            shaderMaterial.SetShaderParameter("dig_offset", 33);
        } 
        else if (drillFromDirection == DrillFromDirection.UP) 
        {
            shaderMaterial.SetShaderParameter("dig_orientation", false);
            shaderMaterial.SetShaderParameter("flip_dig_animation_v", false);
            shaderMaterial.SetShaderParameter("dig_offset", 33);
        }
    }

    public static void SetAnimationProgress(Node2D drillable, float progress) 
    {
        int digOffset = Mathf.RoundToInt((1.0f - progress) * 33);
        AnimatedSprite2D animatedSprite2D = drillable.GetNode<AnimatedSprite2D>("Dirt_AnimatedSprite2D");
        ShaderMaterial shaderMaterial = animatedSprite2D.Material as ShaderMaterial;
        shaderMaterial.SetShaderParameter("dig_offset", digOffset);
    }

    public static void UpdateSolidTileSide(Tile solidTile, SolidTileSurface surface, bool isExposed)
    {
        AnimatedSprite2D animatedSprite2D = solidTile.GetNode<AnimatedSprite2D>("Dirt_AnimatedSprite2D");
        ShaderMaterial shaderMaterial = animatedSprite2D.Material as ShaderMaterial;

        if (surface == SolidTileSurface.Top)
        {
            shaderMaterial.SetShaderParameter("show_top_cover", isExposed);
        }
        else if (surface == SolidTileSurface.Bottom)
        {
            shaderMaterial.SetShaderParameter("show_bottom_cover", isExposed);
        }
        else if (surface == SolidTileSurface.Left)
        {
            shaderMaterial.SetShaderParameter("show_left_cover", isExposed);
        }
        else if (surface == SolidTileSurface.Right)
        {
            shaderMaterial.SetShaderParameter("show_right_cover", isExposed);
        }
    }

    public static void UpdateSolidTileCorner(Tile solidTile, SolidTileCorner corner, SolidTileCornerShape cornerShape)
    {
        AnimatedSprite2D animatedSprite2D = solidTile.GetNode<AnimatedSprite2D>("Dirt_AnimatedSprite2D");
        ShaderMaterial shaderMaterial = animatedSprite2D.Material as ShaderMaterial;

        Vector4 cornerStates = (Vector4) shaderMaterial.GetShaderParameter("corner_states");

        if (corner == SolidTileCorner.TopLeft)
        {
            cornerStates.X = GetCornerStateForShape(cornerShape);
        } else if (corner == SolidTileCorner.TopRight)
        {
            cornerStates.Y = GetCornerStateForShape(cornerShape);
        } else if (corner == SolidTileCorner.BottomLeft)
        {
            cornerStates.Z = GetCornerStateForShape(cornerShape);
        } else if (corner == SolidTileCorner.BottomRight)
        {
            cornerStates.W = GetCornerStateForShape(cornerShape);
        }

        shaderMaterial.SetShaderParameter("corner_states", cornerStates);
    }

    private static float GetCornerStateForShape(SolidTileCornerShape cornerShape)
    {
        if (cornerShape == SolidTileCornerShape.Straight)
        {
            return 0.0f;
        }
        else if (cornerShape == SolidTileCornerShape.Convex)
        {
            return 1.0f;
        }
        else if (cornerShape == SolidTileCornerShape.Concave)
        {
            return -1.0f;
        }
        return 0.0f;
    }
}