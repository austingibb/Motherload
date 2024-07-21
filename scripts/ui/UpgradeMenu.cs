using Godot;

public partial class UpgradeMenu : CanvasLayer
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("exit"))
        {
            this.Visible = false;
        }
    }
}
