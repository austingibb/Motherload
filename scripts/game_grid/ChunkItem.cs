
using Godot;

public enum ChunkItemType
{
	Chest, 
    None
}

public interface ChunkItem
{
    public void Disable();
    public void Enable();
    public Vector2 GetPosition();
}