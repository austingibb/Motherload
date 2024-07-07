using Godot;

public enum TileType
{
    Background,
    Drillable,
    GameGridItem,
    NonDrillable
}

public partial class Tile : StaticBody2D
{
    [Export]
    public TileType tileType;
}