using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class VineInteraction : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    private InputManager inputManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private FireManager fireManager;
    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    private void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.right) * 10;
        Debug.DrawRay(gameObject.transform.position, forward, Color.red);
        int vineMask = LayerMask.GetMask("vine");
        if (Physics2D.Raycast(gameObject.transform.position, gameObject.transform.right, 2, vineMask))
        {
            Debug.Log("rammer vine");
            RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, gameObject.transform.forward, 2, 13);
            Vector3Int vinePos = map.WorldToCell(hit.point);
            TileData data = mapManager.GetTileData(vinePos);
            if (inputManager.PressInteract())
            {
                Debug.Log("trykket e");
                fireManager.SetTileOnFire(vinePos, data);
            }
        }
    }

}
