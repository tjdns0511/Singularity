// In Assets/Scripts/Data/BlockStateData.cs

using UnityEngine;

/// <summary>
/// GDD 4.8.4 - 저장/로드될 블록의 동적 상태입니다.
/// (GDD 4.2.2) MachineStateData 같은 하위 클래스로 확장될 수 있습니다.
/// </summary>
[System.Serializable] // JSON 직렬화를 위해 필수
public class BlockStateData
{
    [Tooltip("원본 BlockData의 ID")]
    public string dataId;

    [Tooltip("블록의 그리드 좌표")]
    public Vector3Int position;

    [Tooltip("블록의 회전값")]
    public Quaternion rotation;

    // 예: MachineStateData로 확장 시
    // public List<InventorySlot> internalBuffer;
    // public float processingTimer;
}