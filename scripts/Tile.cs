using Godot;

public enum TileType
{
    Background,
    Drillable,
    NonDrillable
}

public partial class Tile : StaticBody2D
{
    [Export]
    public TileType tileType;
}