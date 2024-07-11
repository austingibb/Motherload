using System.Collections.Generic;
using Godot;

public class GameGridChunk
{
    public int size;
    public List<List<int?>> tileSetIds;
    public List<List<int?>> itemTileSetIds;

    public GameGridChunk(int size)
    {
        tileSetIds = new List<List<int?>>();
        itemTileSetIds = new List<List<int?>>();

        for (int i = 0; i < size; i++)
        {
            tileSetIds.Add(new List<int?>());
            itemTileSetIds.Add(new List<int?>());
            for (int j = 0; j < size; j++)
            {
                tileSetIds[i].Add(null);
                itemTileSetIds[i].Add(null);
            }
        }
    }

    public void GetTile(Vector2I pos, out int? tileSetId, out int? itemTileSetId)
    {
        tileSetId = tileSetIds[pos.X][pos.Y];
        itemTileSetId = itemTileSetIds[pos.X][pos.Y];
    }

    public void SetTile(Vector2I pos, int tileSetId, int itemTileSetId)
    {
        tileSetIds[pos.X][pos.Y] = tileSetId;
        itemTileSetIds[pos.X][pos.Y] = itemTileSetId;
    }
}