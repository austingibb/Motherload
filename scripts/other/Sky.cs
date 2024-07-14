using Godot;
using System;

public partial class Sky : Node2D
{
    [Export]
    Curve timeToGrayDivision;

    ShaderMaterial shader;

    public override void _Ready()
    {
        shader = GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial;
    }

    public void UpdateSky(double GameTime)
    {
        float time = (float) GameTime / 100.0f;
        time = -time + 1.0f;
        shader.SetShaderParameter("gray_divide", timeToGrayDivision.Sample(time));
    }
}
