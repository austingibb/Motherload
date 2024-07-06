using System.Collections.Generic;

public enum UpgradeType
{
    Drill,
    Armor,
    Battery,
    Jets
}


public class Upgrade {
    public UpgradeType upgradeType;
    public int cost;
    public string description;
    public string name;
}

public class DrillUpgrade : Upgrade
{
    public float drillSpeedMultiplier;
    public DrillType drillType;
}