using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyManager : Node2D
{
    [Export]
    public PackedScene EnemyScene;

    public List<ChunkItem> SpawnEnemies(Vector2 topLeft, Vector2 spawnArea)
    {
        List<ChunkItem> enemies = new List<ChunkItem>();

        for (int i = 0; i < 1; i++)
        {
            if (GD.Randf() > 0.01)
            {
                continue;
            }
                
            float xPos = (float)GD.RandRange((float)topLeft.X, (float)(topLeft.X + spawnArea.X));
            float yPos = (float)GD.RandRange((float)topLeft.Y, (float)(topLeft.Y + spawnArea.Y));
            Enemy enemy = SpawnEnemy(new Vector2(xPos, yPos));

            enemies.Add(enemy);
        }

        return enemies;
    }

    public Enemy SpawnEnemy(Vector2 position)
    {
        Enemy enemy = (Enemy)EnemyScene.Instantiate();
        enemy.GlobalPosition = position;
        enemy.ZIndex = 5;
        AddChild(enemy);

        return enemy;
    }
}
