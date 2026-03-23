using UnityEngine;
using UnityEngine.Tilemaps;

public class TileInteraction : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private FireManager fireManager;

    private InputManager _inputMgr;

    private void Start() => _inputMgr = InputManager.Instance;

    private void Update()
    {
        int vineMask = LayerMask.GetMask("vine");
        RaycastHit2D hitVine = Physics2D.Raycast(transform.position, transform.right, 2, vineMask);

        if (hitVine)
        {
            Vector3Int vinePos = map.WorldToCell(hitVine.point);
            VineTileData vineTileData = mapManager.GetVineTileData(vinePos);

            if (_inputMgr.PressInteract())
                fireManager.SetVineOnFire(vinePos, vineTileData);
        }

        int checkpointMask = LayerMask.GetMask("Checkpoint");
        RaycastHit2D hitCheckpoint = Physics2D.Raycast(transform.position, transform.right, 2, checkpointMask);

        if (hitCheckpoint)
        {
            Vector3Int checkpointPos = map.WorldToCell(hitCheckpoint.point);
            CheckpointTileData checkpointTileData = mapManager.GetCheckpointTileData(checkpointPos);

            if (_inputMgr.PressInteract())
            {
                
            }
        }
    }
}
