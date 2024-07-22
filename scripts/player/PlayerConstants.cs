public enum DrillType
{
    QCarbon = 5,
    Diamond = 4,
    Sapphire = 3,
    Topaz = 2,
    Steel = 1,
    Base = 0
}

public enum BatteryType
{
    HUGE = 5,
    TITANIUM = 4,
    GOLD = 3,
    PINK = 2,
    BASIC = 1,
    RUSTY = 0
}

public enum ArmorType
{
    A,
    B,
    C,
    D,
    F
}


class PlayerConstants
{
    public const float TiltAmount = 0.174533f;
    public const float DragConstant = 0.011f;
    public const float BaseDrillSpeed = 80.0f;
    public const float BaseEnergyLossScale = 3.0f;
    public const float BaseEnergy = 100.0f;
    public const float UnitHeightDigUpResistance = 3;
}
