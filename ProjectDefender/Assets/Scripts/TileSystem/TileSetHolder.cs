using UnityEngine;

/// <summary>
/// A component on the GridBuilder that holds prefab references.
/// Used by TileSlot to identify tile types (e.g., to know if it's a buildable "tileField").
/// </summary>
public class TileSetHolder : MonoBehaviour
{
    public GameObject levelSelectTile;
    
    
    [Header("Common Tiles")]
    public GameObject tileRoad;
    public GameObject tileField;
    public GameObject tileSideway;

    [Header("Corners")] 
    public GameObject tileInnerCorner;
    public GameObject tileInnerCornerSmall;
    public GameObject tileOuterCorner;
    public GameObject tileOuterCornerSmall;

    [Header("Hills")] 
    public GameObject tileHill1;
    public GameObject tileHill2;
    public GameObject tileHill3;
    
    [Header("Bridges")] 
    public GameObject tileBridgeField;
    public GameObject tileBridgeRoad;
    public GameObject tileBridgeSideway;
}