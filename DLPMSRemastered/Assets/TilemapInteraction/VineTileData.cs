using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class VineTileData : ScriptableObject
{
    public TileBase tile;

    public bool canBurn;

    public float spreadIntervall, burnTime;
}
