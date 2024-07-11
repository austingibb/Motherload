
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

public enum TileSetId
{
    // non-resources, backgrounds, non-drillables
    DIRT = 1,
    DIRT_NON_DRILLABLE = 2,
    DIRT_NON_DRILLABLE_STONE_TOP = 3,
    DIRT_BACKGROUND = 4,

    // drillable resources
    IRON = 100,
    COPPER = 101,
    SILVER = 102,
    GOLD = 103,
    DIAMOND = 104,
    LITHIUM = 105,

    // items
    GOLD_CHEST = 200,
    SILVER_CHEST = 201,
    RED_CHEST = 202,
    GREEN_CHEST = 203,
    BROWN_CHEST = 204
}

public class GameGridConstants
{
    // tile ids
    public const int DIRT_TILE_SET_ID = 1;
    public const int DIRT_NON_DRILLABLE_TILE_SET_ID = 13;
    public const int DIRT_BACKGROUND_TILE_SET_ID = 3;
    public const int GOLD_TILE_SET_ID = 4;
    public const int IRON_TILE_SET_ID = 5;
    public const int SILVER_TILE_SET_ID = 6;
    public const int DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID = 7;
    public const int DIAMOND_TILE_SET_ID = 8;
    public const int LITHIUM_TILE_SET_ID = 9;
    public const int COPPER_TILE_SET_ID = 10;

    // item ids
    public const int GOLD_CHEST_TILE_SET_ID = 12;
    public const int SILVER_CHEST_TILE_SET_ID = 17;
    public const int RED_CHEST_TILE_SET_ID = 16;
    public const int GREEN_CHEST_TILE_SET_ID = 15;
    public const int BROWN_CHEST_TILE_SET_ID = 14;

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

    public static TileType MapTileSetIdToTileType(int tileSetId)
    {
        switch (tileSetId)
        {
            case DIRT_TILE_SET_ID:
                return TileType.Drillable;
            case IRON_TILE_SET_ID:
                return TileType.Drillable;
            case SILVER_TILE_SET_ID:
                return TileType.Drillable;
            case GOLD_TILE_SET_ID:
                return TileType.Drillable;
            case DIAMOND_TILE_SET_ID:
                return TileType.Drillable;
            case LITHIUM_TILE_SET_ID:
                return TileType.Drillable;
            case COPPER_TILE_SET_ID:
                return TileType.Drillable;
            case DIRT_NON_DRILLABLE_TILE_SET_ID:
                return TileType.NonDrillable;
            case DIRT_BACKGROUND_TILE_SET_ID:
                return TileType.Background;
            case DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID:
                return TileType.NonDrillable;
            default:
                return TileType.NonDrillable;
        }
    }

    public static int MapChestTypeToTileSetId(ChestType chestType)
    {
        switch (chestType)
        {
            case ChestType.Gold:
                return GOLD_CHEST_TILE_SET_ID;
            case ChestType.Silver:
                return SILVER_CHEST_TILE_SET_ID;
            case ChestType.Red:
                return RED_CHEST_TILE_SET_ID;
            case ChestType.Green:
                return GREEN_CHEST_TILE_SET_ID;
            case ChestType.Brown:
                return BROWN_CHEST_TILE_SET_ID;
            default:
                return -1;
        }
    }
}
