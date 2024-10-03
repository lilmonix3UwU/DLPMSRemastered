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
    private Player player;

    private List<Vector3Int> activeFires = new List<Vector3Int>();
    private InputManager _inputMgr;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        _inputMgr = InputManager.Instance;
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

            VineTileData vineTileData = mapManager.GetVineTileData(tilePos);

            if(vineTileData != null && vineTileData.canBurn && player.curTorch == 1)
            {
                SetVineOnFire(tilePos, vineTileData);
            }

        }

    }

    public void SetVineOnFire(Vector3Int tilePos, VineTileData vineTileData)
    {
        Fire newFire = Instantiate(firePrefab);
        newFire.transform.position = map.GetCellCenterWorld(tilePos);
        newFire.StartBurning(tilePos, vineTileData, this);

        activeFires.Add(tilePos);

    }
    private void Update()
    {
        Vector2 playerDirection = transform.right;
        
        if (player.GetComponent<Player>().facingDir < 0)
        {
            playerDirection = Vector2.left;
        }
        else if (player.GetComponent<Player>().facingDir > 0)
        {
            playerDirection = Vector2.right;
        }


        int vineMask = LayerMask.GetMask("vine");
        RaycastHit2D hitVine = Physics2D.Raycast(player.transform.position, playerDirection, 2, vineMask);

        if (hitVine)
        {
            Vector3Int vinePos = map.WorldToCell(hitVine.point);
            VineTileData vineTileData = mapManager.GetVineTileData(vinePos);

            if (_inputMgr.PressInteract())
            {
                if (vineTileData != null && vineTileData.canBurn && player.curTorch == 1)
                {
                    SetVineOnFire(vinePos, vineTileData);
                }                
            }
        }
    }
}
