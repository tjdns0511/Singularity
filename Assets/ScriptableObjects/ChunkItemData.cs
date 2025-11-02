// In Assets/ScriptableObjects/ChunkItemData.cs

using UnityEngine;

/// <summary>
/// GDD 4.3.1 / GDD 4.4.2 - '청크' 자체를 아이템화한 데이터입니다.
/// ItemData를 상속받아 인벤토리에 보관할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "NewChunkItemData", menuName = "Singularity/Data/Chunk Item Data")]
public class ChunkItemData : ItemData
{
    [Header("Chunk Properties")]
    [Tooltip("월드에 설치(PlaceChunk)될 청크의 원본 프리팹 (GDD 4.3.1)")]
    public GameObject chunkPrefab;
}