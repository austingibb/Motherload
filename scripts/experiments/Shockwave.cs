using Godot;
using System;

public partial class Shockwave : Sprite2D
{
    public ShaderMaterial shader;

    public override void _Ready()
    {
        shader = this.Material as ShaderMaterial;
    }

    public override void _Process(double delta)
    {
        Vector2 mousePosition = SpritePositionToTexturePosition(GetLocalMousePosition());
        mousePosition.X = Mathf.Round(mousePosition.X);
        mousePosition.Y = Mathf.Round(mousePosition.Y);
        shader.SetShaderParameter("center", mousePosition);
    }

    public Vector2 SpritePositionToTexturePosition(Vector2 position)
    {
        return new Vector2(
            position.X + Texture.GetWidth() / 2,
            position.Y + Texture.GetHeight() / 5
        );
    }
}
