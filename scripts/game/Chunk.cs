using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkArea : Area2D
{
    public Vector2I chunkPosition;
    public Vector2 startingChunkPos;
    public Vector2 chunkTopLeft;
    public Vector2I tileSize;
    public int chunkSize;

	public ChunkArea(Vector2I chunkPosition, int chunkSize, Vector2 startingChunkPos, Vector2I tileSize)
    {
        this.chunkPosition = chunkPosition;
        this.chunkSize = chunkSize;
        this.startingChunkPos = startingChunkPos;
        this.chunkTopLeft = new Vector2(startingChunkPos.X + chunkPosition.X * chunkSize * tileSize.X, startingChunkPos.Y + chunkPosition.Y * chunkSize * tileSize.Y);
        this.tileSize = tileSize;

        SetCollisionDimension();
	}

    private void SetCollisionDimension()
    {
        CollisionShape2D collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
        RectangleShape2D rectangleShape2D = (RectangleShape2D)collisionShape2D.Shape;
        rectangleShape2D.Size = new Vector2(chunkSize * tileSize.X, chunkSize * tileSize.Y);
        Position = new Vector2(startingChunkPos.X + chunkPosition.X * chunkSize * tileSize.X, startingChunkPos.Y + chunkPosition.Y * chunkSize * tileSize.Y);
    }
}