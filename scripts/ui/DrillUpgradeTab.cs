using Godot;
using System;
using System.Collections.Generic;

public partial class DrillUpgradeTab : UpgradeTab
{
    DrillColorReplacements drillColorReplacements;

    public override void _Ready()
    {
        drillColorReplacements = GetNode<DrillColorReplacements>("%drillColorReplacements");
        base._Ready();
    }

    public override List<Upgrade> GetUpgrades()
    {
        return UpgradeConstants.drillUpgrades;
    }

    public override void SetupUpgradeInfo(TextureRect textureRect, Label label, Upgrade upgrade)
    {
        base.SetupUpgradeInfo(textureRect, label, upgrade);
        SetDrillColorReplacements(textureRect, (DrillType)upgrade.Rank());
    }

    public override void SetupCurrentUpgradeInfo(TextureRect textureRect, Label label, Upgrade upgrade)
    {
        base.SetupCurrentUpgradeInfo(textureRect, label, upgrade);
        SetDrillColorReplacements(textureRect, (DrillType)upgrade.Rank());
    }

    public void SetDrillColorReplacements(TextureRect textureRect, DrillType drillType)
    {
        ShaderMaterial shaderMaterial = textureRect.Material as ShaderMaterial;
        PlayerShaderManager.UpdateColorReplaceParameters(shaderMaterial, 
            drillColorReplacements.drillColorsToReplace, 
            drillColorReplacements.DrillTypeToColorReplacement(drillType));
    }
}
