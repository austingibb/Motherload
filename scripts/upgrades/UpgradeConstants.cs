using System.Collections.Generic;

public class UpgradeConstants
{
    public static List<Upgrade> batteryUpgrades = new List<Upgrade>
    {
        new BatteryUpgrade
        {
            upgradeType = UpgradeType.Battery,
            batteryType = BatteryType.RUSTY,
            cost = 0,
            description = "Low battery capacity",
            name = "Old Battery",
            capacity = 100
        },
        new BatteryUpgrade
        {
            upgradeType = UpgradeType.Battery,
            batteryType = BatteryType.BASIC,
            cost = 1_000,
            description = "Increase battery capacity by 50%",
            name = "Standard Battery",
            capacity = 150
        },
        new BatteryUpgrade
        {
            upgradeType = UpgradeType.Battery,
            batteryType = BatteryType.PINK,
            cost = 2_000,
            description = "Increase battery capacity by 75%",
            name = "Overcharged Standard Battery",
            capacity = 175
        },
        new BatteryUpgrade
        {
            upgradeType = UpgradeType.Battery,
            batteryType = BatteryType.GOLD,
            cost = 5_000,
            description = "Increase battery capacity by 100%",
            name = "Gold Battery",
            capacity = 200
        },
        new BatteryUpgrade
        {
            upgradeType = UpgradeType.Battery,
            batteryType = BatteryType.TITANIUM,
            cost = 20_000,
            description = "Increase battery capacity by 150%",
            name = "Titanium Battery",
            capacity = 250
        },
        new BatteryUpgrade
        {
            upgradeType = UpgradeType.Battery,
            batteryType = BatteryType.HUGE,
            cost = 50_000,
            description = "Increase battery capacity by 200%",
            name = "Huge Battery",
            capacity = 300
        }
    };

    public static List<Upgrade> drillUpgrades = new List<Upgrade>
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
            cost = 1_000,
            description = "Increase drill speed by 25%",
            name = "Steel Drill",
            drillSpeedMultiplier = 1.25f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Topaz,
            cost = 2_000,
            description = "Increase drill speed by 50%",
            name = "Topaz Drill",
            drillSpeedMultiplier = 1.5f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Sapphire,
            cost = 5_000,
            description = "Increase drill speed by 75%",
            name = "Saphphire Drill",
            drillSpeedMultiplier = 1.75f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.Diamond,
            cost = 20_000,
            description = "Increase drill speed by 100%",
            name = "Diamond Drill",
            drillSpeedMultiplier = 2.0f
        },
        new DrillUpgrade
        {
            upgradeType = UpgradeType.Drill,
            drillType = DrillType.QCarbon,
            cost = 50_000,
            description = "Increase drill speed by 150%",
            name = "QCarbon Drill",
            drillSpeedMultiplier = 2.25f
        }
    };
}

