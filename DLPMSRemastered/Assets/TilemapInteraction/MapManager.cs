using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private List<VineTileData> vineTileDatas;
    [SerializeField] private List<CheckpointTileData> checkpointTileDatas;

    private Dictionary<TileBase, VineTileData> dataFromVineTiles;
    private Dictionary<TileBase, CheckpointTileData> dataFromCheckpointTiles;

    private void Awake()
    {
        dataFromVineTiles = new Dictionary<TileBase, VineTileData>();

        foreach (VineTileData vineTileData in vineTileDatas)
        {
            dataFromVineTiles.Add(vineTileData.tile, vineTileData);
        }

        dataFromCheckpointTiles = new Dictionary<TileBase, CheckpointTileData>();

        foreach (CheckpointTileData checkpointTileData in checkpointTileDatas)
        {
            dataFromCheckpointTiles.Add(checkpointTileData.tile, checkpointTileData);
        }
    }

    public VineTileData GetVineTileData(Vector3Int tilePos)
    {
        TileBase tile = map.GetTile(tilePos);

        if (tile == null)
            return null;
        else
            return dataFromVineTiles[tile];


    }

    public CheckpointTileData GetCheckpointTileData(Vector3Int tilePos)
    {
        TileBase tile = map.GetTile(tilePos);

        if (tile == null)
            return null;
        else
            return dataFromCheckpointTiles[tile];
    }
}
