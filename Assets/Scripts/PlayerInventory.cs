using UnityEngine;
using System.Collections.Generic;
using System.Linq; // FindIndex 등 사용

/// <summary>
/// 플레이어의 인벤토리를 관리합니다. (청크 인벤토리 포함)
/// </summary>
public class PlayerInventory : MonoBehaviour // 싱글톤으로 만들거나, 플레이어 오브젝트에 직접 붙일 수 있음
{
    // 기획 문서 4.5: `List<InventorySlot> items;` // 일반 아이템 인벤토리 (구현 필요 시 추가)
    [SerializeField]
    private List<InventorySlot> chunkInventory = new List<InventorySlot>();

    // 인벤토리 변경 시 발생할 이벤트 (UI 업데이트 등)
    public event System.Action OnInventoryChanged;

    // --- 청크 인벤토리 관리 ---

    /// <summary>
    /// 청크 인벤토리에 아이템을 추가합니다.
    /// </summary>
    /// <param name="chunkItemData">추가할 청크 아이템 데이터</param>
    /// <param name="quantity">추가할 수량</param>
    public void AddChunkItem(ChunkItemData chunkItemData, int quantity = 1)
    {
        if (chunkItemData == null || quantity <= 0) return;

        // 이미 인벤토리에 같은 아이템이 있는지 확인
        int existingSlotIndex = chunkInventory.FindIndex(slot => slot.itemDataRef == chunkItemData);

        if (existingSlotIndex != -1)
        {
            // 있으면 수량만 증가
            InventorySlot existingSlot = chunkInventory[existingSlotIndex];
            existingSlot.quantity += quantity;
            // chunkInventory[existingSlotIndex] = existingSlot; // 구조체(struct)일 경우 다시 할당해야 함. 클래스면 필요 없음.
            Debug.Log($"Added {quantity} of {chunkItemData.resourceName} (Total: {existingSlot.quantity})");
        }
        else
        {
            // 없으면 새 슬롯 추가
            chunkInventory.Add(new InventorySlot(chunkItemData, quantity));
            Debug.Log($"Added new item {chunkItemData.resourceName} x{quantity}");
        }

        // 인벤토리 변경 이벤트 발생
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 청크 인벤토리에서 아이템을 제거합니다.
    /// </summary>
    /// <param name="chunkItemData">제거할 청크 아이템 데이터</param>
    /// <param name="quantity">제거할 수량</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveChunkItem(ChunkItemData chunkItemData, int quantity = 1)
    {
        if (chunkItemData == null || quantity <= 0) return false;

        int slotIndex = chunkInventory.FindIndex(slot => slot.itemDataRef == chunkItemData);

        if (slotIndex != -1)
        {
            InventorySlot slot = chunkInventory[slotIndex];
            if (slot.quantity >= quantity)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                {
                    // 수량이 0 이하면 슬롯 제거
                    chunkInventory.RemoveAt(slotIndex);
                    Debug.Log($"Removed item {chunkItemData.resourceName} from inventory.");
                }
                else
                {
                    // 수량만 감소 (클래스면 자동 반영, 구조체면 다시 할당)
                    Debug.Log($"Removed {quantity} of {chunkItemData.resourceName} (Remaining: {slot.quantity})");
                }

                OnInventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning($"Cannot remove {quantity} of {chunkItemData.resourceName}: Only have {slot.quantity}.");
                return false; // 수량 부족
            }
        }
        else
        {
            Debug.LogWarning($"Cannot remove {chunkItemData.resourceName}: Item not found in chunk inventory.");
            return false; // 아이템 없음
        }
    }

    /// <summary>
    /// 특정 청크 아이템을 가지고 있는지 확인합니다.
    /// </summary>
    /// <param name="chunkItemData">확인할 아이템</param>
    /// <param name="minQuantity">필요한 최소 수량</param>
    /// <returns>소지 여부</returns>
    public bool HasChunkItem(ChunkItemData chunkItemData, int minQuantity = 1)
    {
        InventorySlot slot = chunkInventory.FirstOrDefault(s => s.itemDataRef == chunkItemData);
        return slot != null && slot.quantity >= minQuantity;
    }


    /// <summary>
    /// 현재 청크 인벤토리 내용을 반환합니다. (UI 표시용)
    /// </summary>
    public List<InventorySlot> GetChunkInventory()
    {
        // 읽기 전용으로 반환하거나 복사본을 반환하는 것이 더 안전할 수 있음
        return chunkInventory;
    }


    // --- 저장/로드 ---

    /// <summary>
    /// 청크 인벤토리 데이터를 저장 가능한 형태로 반환합니다.
    /// </summary>
    public List<InventorySlotSaveData> GetChunkInventorySaveData()
    {
        List<InventorySlotSaveData> saveData = new List<InventorySlotSaveData>();
        foreach (var slot in chunkInventory)
        {
            // ItemData의 ID와 수량만 저장
            if (slot.itemDataRef != null) // 혹시 모를 null 체크
            {
                saveData.Add(new InventorySlotSaveData(slot.itemDataRef.name, slot.quantity)); // ItemData의 name을 ID로 사용
            }
        }
        return saveData;
    }

    /// <summary>
    /// 저장된 데이터로부터 청크 인벤토리를 복원합니다.
    /// </summary>
    public void RestoreChunkInventory(List<InventorySlotSaveData> savedData)
    {
        chunkInventory.Clear();
        if (savedData == null) return;

        foreach (var data in savedData)
        {
            // ID로 DataManager에서 ChunkItemData 찾기
            ChunkItemData itemData = DataManager.Instance.GetChunkItemData(data.itemId); // GetChunkItemData 사용
            if (itemData != null)
            {
                chunkInventory.Add(new InventorySlot(itemData, data.quantity));
            }
            else
            {
                Debug.LogError($"Could not restore chunk inventory item: ChunkItemData with ID '{data.itemId}' not found.");
            }
        }
        OnInventoryChanged?.Invoke(); // 로드 완료 후 UI 업데이트 등
        Debug.Log($"Restored {chunkInventory.Count} chunk inventory slots.");
    }
}


// --- 필요한 보조 클래스 (별도 파일로 분리하는 것이 좋음) ---

/// <summary>
/// 인벤토리 슬롯 데이터 (아이템 참조 및 수량)
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public ItemData itemDataRef; // 어떤 아이템인지 ScriptableObject 참조
    public int quantity;

    public InventorySlot(ItemData itemData, int qty)
    {
        itemDataRef = itemData;
        quantity = qty;
    }
}

/// <summary>
/// 인벤토리 슬롯 저장을 위한 데이터 구조 (ScriptableObject 직접 참조 대신 ID 사용)
/// </summary>
[System.Serializable]
public class InventorySlotSaveData
{
    public string itemId; // ItemData의 ID (name 또는 별도 id 필드)
    public int quantity;

    public InventorySlotSaveData(string id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}