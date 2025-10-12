using UnityEngine;

[CreateAssetMenu(fileName = "New chunk", menuName = "Singularity/Data/Resource/Item")]
public class ChunkItemData : ItemData
{
    [Header("Chunk Creation Properties")]
    [Tooltip("자원 종류")]
    public ResourceData resourceType;

    [Tooltip("자원 품질")]
    [Range(0f, 10f)]
    public float resourceQuality = 1.0f;
}
