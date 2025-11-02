// In Assets/ScriptableObjects/ItemData.cs

using UnityEngine;

/// <summary>
/// GDD 4.4.2 - 인벤토리에 '개수'로 쌓일 수 있는 고체 아이템 데이터입니다.
/// ResourceData를 상속받습니다.
/// (예: 원소, 부산물, 철판, 구리선 등)
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "Singularity/Data/Item Data")]
public class ItemData : ResourceData
{
    [Header("Item Properties")]
    [Tooltip("한 슬롯에 쌓일 수 있는 최대 개수")]
    public int maxStackSize = 100;
}