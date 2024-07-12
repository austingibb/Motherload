using Godot;
using System;

public partial class UpgradeStation : StaticBody2D
{
    [Signal]
    public delegate void upgradeEventHandler();

    public UpgradeMenu upgradeMenu;

    public override void _Ready()
    {
        upgradeMenu = GetNode<UpgradeMenu>("%UpgradeMenu");
        upgradeMenu.Visible = false;
    }

    private void _on_upgrade_zone_entered(Node2D body)
	{
		EmitSignal(SignalName.upgrade);
        upgradeMenu.Visible = true;
	}

    private void _on_upgrade_zone_exited(Node2D body)
	{
		EmitSignal(SignalName.upgrade);
        upgradeMenu.Visible = false;
	}
}
