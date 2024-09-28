using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class FireManager : MonoBehaviour
{


    [SerializeField]
    private Tilemap map;

    [SerializeField]
    private MapManager mapManager;

    [SerializeField]
    private Fire firePrefab;
    [SerializeField]
    private GameObject player;

    private List<Vector3Int> activeFires = new List<Vector3Int>();
    private InputManager inputManager;

    private void Start()
    {
        player = FindObjectOfType<Player>().gameObject;
        inputManager = InputManager.Instance;
    }


    public void FinishedBurning(Vector3Int position)
    {
        map.SetTile(position, null);
        activeFires.Remove(position);
    }

    public void TryToSpread(Vector3Int position)
    {
        for (int x = position.x -1; x < position.x + 2 ; x++)
        {
            for (int y = position.y - 1; y < position.y + 2; y++)
            {
                TryToBurnTile(new Vector3Int(x, y, 0));
            }
        }


         void TryToBurnTile(Vector3Int tilePosition)
        {
            if (activeFires.Contains(tilePosition)) return;

            TileData data = mapManager.GetTileData(tilePosition);

            if(data != null && data.canBurn)
            {
                SetTileOnFire(tilePosition, data);
            }

        }

    }

     public void SetTileOnFire(Vector3Int tilePosition, TileData data)
    {
        Fire newFire = Instantiate(firePrefab);
        newFire.transform.position = map.GetCellCenterWorld(tilePosition);
        newFire.StartBurning(tilePosition, data, this);

        activeFires.Add(tilePosition);

    }




    private void Update()
    {
        Vector2 playerDirection = transform.right;
        Vector3 forward = player.transform.TransformDirection(Vector3.right) * 10;
        Debug.DrawRay(player.transform.position, forward, Color.red);
        int vineMask = LayerMask.GetMask("vine");
        if (player.GetComponent<Player>().facingDir < 0)
        {
            playerDirection = Vector2.left;
            
        }
        else if (player.GetComponent<Player>().facingDir > 0)
        {
            playerDirection = Vector2.right;
        }


        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, playerDirection, 2, vineMask);
        if (hit)
        {
            Vector3Int vinePos = map.WorldToCell(hit.point);
            TileData data = mapManager.GetTileData(vinePos);

            if (inputManager.PressInteract())
            {
                if (data != null && data.canBurn)
                {
                    SetTileOnFire(vinePos, data);
                }                
            }
        }
    }


}
