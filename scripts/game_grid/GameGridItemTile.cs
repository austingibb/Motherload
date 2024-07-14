using Godot;

public partial class GameGridItemTile : Tile
{
	public ChunkItemType gameGridItemType;

	[Export]
	public PackedScene SpawnableItem;

	public override void _Ready()
	{
		this.tileType = TileType.GameGridItem;
		base._Ready();
	}

	public Node2D SpawnItem()
	{
		if (SpawnableItem != null)
		{
			Node2D item = SpawnableItem.Instantiate() as Node2D;
			GetParent().GetParent().GetNode<Node2D>("Items").AddChild(item);
			item.GlobalPosition = GlobalPosition;
			QueueFree();
            return item;
		}
        return null;
	}
}
