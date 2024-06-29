using Godot;
using System;

public partial class ProgressBarNinePatch : Node2D
{
    // Nodes
    [Export]
    public NinePatchRect background;
    [Export]
    public NinePatchRect fill;
    public Node2D maxWidthMarker;

    // Member variables
    [Export]
    public float MaxValue;
    [Export]
    public float Value;
    [Export]
    public bool RoundedValue;
    [Export]
    public bool RoundedPosition;

    public override void _Ready()
    {
        maxWidthMarker = GetNode<Node2D>("MaxWidthMarker");
        SetValue(100);
    }

    public void SetValue(float value)
    {
        if (RoundedValue)
        {
            value = Mathf.Round(value);
        }
        Value = value;
        UpdateFill();
    }

    public void UpdateFill()
    {
        float fillWidth = (float)Value / MaxValue * maxWidthMarker.Position.X;
        Control fillControl = (Control) fill;
        fillWidth = RoundedPosition ? Mathf.Round(fillWidth) : fillWidth;
        fillControl.Size = new Vector2(fillWidth, fillControl.Size.Y);
    }
}
