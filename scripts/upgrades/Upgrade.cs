using System.Collections.Generic;

public enum UpgradeType
{
    Drill,
    Armor,
    Battery,
    Jets
}


public abstract class Upgrade {
    public UpgradeType upgradeType;
    public int cost;
    public string description;
    public string name;

    public abstract int Rank();
}

public class DrillUpgrade : Upgrade
{
    public float drillSpeedMultiplier;
    public DrillType drillType;

    public override int Rank()
    {
        return (int) drillType;
    }
}

public class BatteryUpgrade : Upgrade
{
    public float capacity;
    public BatteryType batteryType;

    public override int Rank()
    {
        return (int) batteryType;
    }
}