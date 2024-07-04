
using System.Collections.Generic;

public enum DrillableType
{
    DIRT,
    IRON,
    SILVER,
    GOLD,
    DIAMOND,
    LITHIUM,
    COPPER,
    NONE
}

public class DrillableConstants
{
    public const int DIRT_TILE_SET_ID = 1;
    public const int DIRT_NON_DRILLABLE_TILE_SET_ID = 2;
    public const int DIRT_BACKGROUND_TILE_SET_ID = 3;
    public const int GOLD_TILE_SET_ID = 4;
    public const int IRON_TILE_SET_ID = 5;
    public const int SILVER_TILE_SET_ID = 6;
    public const int DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID = 7;
    public const int DIAMOND_TILE_SET_ID = 8;
    public const int LITHIUM_TILE_SET_ID = 9;
    public const int COPPER_TILE_SET_ID = 10;

    public static Dictionary<DrillableType, int> itemPrices = new Dictionary<DrillableType, int>
    {
        { DrillableType.IRON, 10 },
        { DrillableType.COPPER, 20},
        { DrillableType.GOLD, 50 },
        { DrillableType.SILVER, 30 },
        { DrillableType.DIAMOND, 300 },
        { DrillableType.LITHIUM, 100 }
    };

    public static int MapDrillableTypeToTileSetId(DrillableType drillableType)
    {
        switch (drillableType)
        {
            case DrillableType.DIRT:
                return DIRT_TILE_SET_ID;
            case DrillableType.IRON:
                return IRON_TILE_SET_ID;
            case DrillableType.COPPER:
                return COPPER_TILE_SET_ID;
            case DrillableType.GOLD:
                return GOLD_TILE_SET_ID;
            case DrillableType.SILVER:
                return SILVER_TILE_SET_ID;
            case DrillableType.DIAMOND:
                return DIAMOND_TILE_SET_ID;
            case DrillableType.LITHIUM:
                return LITHIUM_TILE_SET_ID;
            case DrillableType.NONE:
                return -1;
            default:
                return DIRT_TILE_SET_ID;
        }
    }
}
