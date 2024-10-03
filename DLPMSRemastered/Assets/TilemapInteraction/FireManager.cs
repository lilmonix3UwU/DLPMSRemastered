using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FireManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;

    [SerializeField]
    private MapManager mapManager;

    [SerializeField]
    private Fire firePrefab;

    [SerializeField]
    private GameObject checkpointFire;

    [SerializeField]
    private Player player;

    private List<Vector3Int> activeFires = new List<Vector3Int>();
    private InputManager inputManager;

    private void Start()
    {
        player = FindObjectOfType<Player>();
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


        void TryToBurnTile(Vector3Int tilePos)
        {
            if (activeFires.Contains(tilePos)) return;

            VineTileData data = mapManager.GetVineTileData(tilePos);

            if(data != null && data.canBurn && player.curTorch == 1)
            {
                SetVineOnFire(tilePos, data);
            }

        }

    }

    public void SetVineOnFire(Vector3Int tilePos, VineTileData data)
    {
        Fire newFire = Instantiate(firePrefab);
        newFire.transform.position = map.GetCellCenterWorld(tilePos);
        newFire.StartBurning(tilePos, data, this);

        activeFires.Add(tilePos);

    }

    public void SetCheckpointOnFire(Vector3Int tilePos, CheckpointTileData data)
    {
        Vector3 offset = new Vector3(0, 2, 0);

        Instantiate(checkpointFire, map.GetCellCenterWorld(tilePos) + offset, Quaternion.identity);

        
    }

    private void Update()
    {
        Vector2 playerDirection = transform.right;
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
            VineTileData data = mapManager.GetVineTileData(vinePos);

            if (inputManager.PressInteract())
            {
                if (data != null && data.canBurn && player.curTorch == 1)
                {
                    SetVineOnFire(vinePos, data);
                }                
            }
        }
    }
}
