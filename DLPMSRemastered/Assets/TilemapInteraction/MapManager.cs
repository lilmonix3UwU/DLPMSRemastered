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






    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);

            TileBase clickedTile = map.GetTile(gridPosition);

           // map.SetTileFlags(gridPosition, TileFlags.None);
          //  map.SetColor(gridPosition, Color.black);

         //   float walkingSpeed = dataFromTiles[clickedTile].walkingSpeed;






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
