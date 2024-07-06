using System.Collections.Generic;

public class UpgradeConstants
{
    public static List<Upgrade> upgrades = new List<Upgrade>
    {
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Base,
            cost = 0,
            description = "Base drill speed",
            name = "Basic Drill",
            drillSpeedMultiplier = 1.0f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Steel,
            cost = 1000,
            description = "Increase drill speed by 25%",
            name = "Steel Drill",
            drillSpeedMultiplier = 1.25f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Topaz,
            cost = 2000,
            description = "Increase drill speed by 50%",
            name = "Topaz Drill",
            drillSpeedMultiplier = 1.5f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Sapphire,
            cost = 10_000,
            description = "Increase drill speed by 75%",
            name = "Saphphire Drill",
            drillSpeedMultiplier = 1.75f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Diamond,
            cost = 50_000,
            description = "Increase drill speed by 100%",
            name = "Diamond Drill",
            drillSpeedMultiplier = 2.0f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.QCarbon,
            cost = 500_000,
            description = "Increase drill speed by 150%",
            name = "QCarbon Drill",
            drillSpeedMultiplier = 2.5f
        }
    };
}

