using UnityEngine;

public class BlockObject : MonoBehaviour
{
    public BlockData data;
    public Vector3Int gridPosition;

    public virtual void Initialize(BlockData blockData, Vector3Int position)
    {
        this.data = blockData;
        this.gridPosition = position;
    }
}
