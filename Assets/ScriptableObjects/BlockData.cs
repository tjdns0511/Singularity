using UnityEngine;



public abstract class BlockData : ScriptableObject
{
    [Header("Common Block Information")]
    public string blockID;
    public string blockName;
    public BlockType type;
    public GameObject prefab;
    public Vector3Int blockSize = Vector3Int.one;
    public ItemData returnedItem;
    public bool breakable = true;
}