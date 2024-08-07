
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
    BAUXITE,
    RHODIUM,
    COAL,
    URANIUM,
    AGATE,
    SAPPHIRE,
    EMERALD,
    RUBY,
    NONE
}

public class GameGridConstants
{
    // tile ids
    public const int DIRT_TILE_SET_ID = 1;
    public const int DIRT_NON_DRILLABLE_TILE_SET_ID = 13;
    public const int AGATE_TILE_SET_ID = 2;
    public const int DIRT_BACKGROUND_TILE_SET_ID = 3;
    public const int GOLD_TILE_SET_ID = 4;
    public const int IRON_TILE_SET_ID = 5;
    public const int SILVER_TILE_SET_ID = 6;
    public const int DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID = 7;
    public const int DIAMOND_TILE_SET_ID = 8;
    public const int LITHIUM_TILE_SET_ID = 9;
    public const int COPPER_TILE_SET_ID = 10;
    public const int BAUXITE_TILE_SET_ID = 11;
    public const int COAL_TILE_SET_ID = 18;
    public const int EMERALD_TILE_SET_ID = 19;
    public const int RHODIUM_TILE_SET_ID = 20;
    public const int RUBY_TILE_SET_ID = 21;
    public const int SAPPHIRE_TILE_SET_ID = 22;
    public const int URANIUM_TILE_SET_ID = 23;

    // item ids
    public const int GOLD_CHEST_TILE_SET_ID = 12;
    public const int SILVER_CHEST_TILE_SET_ID = 17;
    public const int RED_CHEST_TILE_SET_ID = 16;
    public const int GREEN_CHEST_TILE_SET_ID = 15;
    public const int BROWN_CHEST_TILE_SET_ID = 14;

    public static Dictionary<DrillableType, int> itemPrices = new Dictionary<DrillableType, int>
    {
        { DrillableType.BAUXITE, 10 },
        { DrillableType.IRON, 20 },
        { DrillableType.COPPER, 50},
        { DrillableType.SILVER, 100 },
        { DrillableType.GOLD, 200 },
        { DrillableType.RHODIUM, 500 },

        { DrillableType.AGATE, 100 },
        { DrillableType.SAPPHIRE, 200 },
        { DrillableType.EMERALD, 500 },
        { DrillableType.RUBY, 800 },
        { DrillableType.DIAMOND, 1500 },

        { DrillableType.COAL , 50 },
        { DrillableType.LITHIUM, 100 },
        { DrillableType.URANIUM, 200 },
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
            case DrillableType.BAUXITE:
                return BAUXITE_TILE_SET_ID;
            case DrillableType.RHODIUM:
                return RHODIUM_TILE_SET_ID;
            case DrillableType.COAL:
                return COAL_TILE_SET_ID;
            case DrillableType.EMERALD:
                return EMERALD_TILE_SET_ID;
            case DrillableType.RUBY:
                return RUBY_TILE_SET_ID;
            case DrillableType.SAPPHIRE:
                return SAPPHIRE_TILE_SET_ID;
            case DrillableType.URANIUM:
                return URANIUM_TILE_SET_ID;
            case DrillableType.AGATE:
                return AGATE_TILE_SET_ID;
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
            case BAUXITE_TILE_SET_ID:
                return TileType.Drillable;
            case RHODIUM_TILE_SET_ID:
                return TileType.Drillable;
            case COAL_TILE_SET_ID:
                return TileType.Drillable;
            case EMERALD_TILE_SET_ID:
                return TileType.Drillable;
            case RUBY_TILE_SET_ID:
                return TileType.Drillable;
            case SAPPHIRE_TILE_SET_ID:
                return TileType.Drillable;
            case URANIUM_TILE_SET_ID:
                return TileType.Drillable;
            case AGATE_TILE_SET_ID:
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
