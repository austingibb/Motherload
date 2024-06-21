using Godot;

public enum DrillableSurface
{
    Top,
    Bottom
}

public enum DrillableSide 
{
    Left,
    Right,
    None
}

public enum DrillableCornerShape
{
    Straight,
    Convex,
    Concave,
    None
}

public partial class DrillableShaderManager : GodotObject
{
    public static void UpdateDrillableCorner(Node2D drillable, DrillableSurface surface, bool isExposed,
        DrillableSide side = DrillableSide.None, DrillableCornerShape cornerShape = DrillableCornerShape.None)
    {
        AnimatedSprite2D animatedSprite2D = drillable.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        ShaderMaterial shaderMaterial = animatedSprite2D.Material as ShaderMaterial;

        if (surface == DrillableSurface.Top)
        {
            shaderMaterial.SetShaderParameter("show_top_cover", isExposed);
        }
        else if (surface == DrillableSurface.Bottom)
        {
            shaderMaterial.SetShaderParameter("show_bottom_cover", isExposed);
        }

        Vector4 cornerStates = (Vector4) shaderMaterial.GetShaderParameter("corner_states");

        if (!isExposed)
        {
            if (surface == DrillableSurface.Top)
            {
                cornerStates.X = 0.0f;
                cornerStates.Y = 0.0f;
            }
            else if (surface == DrillableSurface.Bottom)
            {
                cornerStates.Z = 0.0f;
                cornerStates.W = 0.0f;
            }
        } else
        {
            if (side == DrillableSide.None || cornerShape == DrillableCornerShape.None)
            {
                GD.PrintErr("DrillableShaderManager.UpdateDrillableCorner: If setting exposed surface, must specify both side and corner shape.");
                return;
            }

            if (surface == DrillableSurface.Top && side == DrillableSide.Left)
            {
                cornerStates.X = GetCornerStateForShape(cornerShape);
            }
            else if (surface == DrillableSurface.Top && side == DrillableSide.Right)
            {
                cornerStates.Y = GetCornerStateForShape(cornerShape);
            }
            else if (surface == DrillableSurface.Bottom && side == DrillableSide.Left)
            {
                cornerStates.Z = GetCornerStateForShape(cornerShape);
            }
            else if (surface == DrillableSurface.Bottom && side == DrillableSide.Right)
            {
                cornerStates.W = GetCornerStateForShape(cornerShape);
            }
        }

        shaderMaterial.SetShaderParameter("corner_states", cornerStates);
    }

    private static float GetCornerStateForShape(DrillableCornerShape cornerShape)
    {
        if (cornerShape == DrillableCornerShape.Straight)
        {
            return 0.0f;
        }
        else if (cornerShape == DrillableCornerShape.Convex)
        {
            return 1.0f;
        }
        else if (cornerShape == DrillableCornerShape.Concave)
        {
            return -1.0f;
        }
        return 0.0f;
    }
}