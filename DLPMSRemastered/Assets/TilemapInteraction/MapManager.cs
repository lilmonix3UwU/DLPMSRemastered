using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;



    [SerializeField]
    private List<TileData> tileDatas;



    private Dictionary<TileBase, TileData> dataFromTiles;





    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas)
        {
            dataFromTiles.Add(tileData.tile, tileData);
        }
    }

    public TileData GetTileData(Vector3Int tilePosition)
    {
        TileBase tile = map.GetTile(tilePosition);

        if (tile == null)
            return null;
        else
            return dataFromTiles[tile];


    }


  


}
