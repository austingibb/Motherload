using Godot;
using System;
using System.Collections.Generic;

public partial class BatteryUpgradeTab : UpgradeTab
{
    public override void _Ready()
    {
        base._Ready();
    }

    public override void SetupCurrentUpgradeInfo(TextureRect textureRect, Label label, Upgrade upgrade)
    {
        base.SetupCurrentUpgradeInfo(textureRect, label, upgrade);
        textureRect.Texture = upgradeIcons[upgrade.Rank() - 1].Texture;
    }

    public override List<Upgrade> GetUpgrades()
    {
        return UpgradeConstants.batteryUpgrades;
    }
}
