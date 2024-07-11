using Godot;

public enum TileType
{
    Background,
    Drillable,
    GameGridItem,
    NonDrillable,
    None
}

public partial class Tile : StaticBody2D
{
    [Export]
    public TileType tileType;

    public override void _Ready()
    {
        if (GetParent() is GameGrid)
        {
            GameGrid gameGrid = GetParent() as GameGrid;
            gameGrid._on_tile_ready(this);
        }
     }
}