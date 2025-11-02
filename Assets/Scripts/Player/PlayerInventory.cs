// In Assets/Scripts/PlayerInventory.cs

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 인벤토리 데이터를 관리합니다. (GDD 4.5.1)
/// 핫바(일반 아이템)와 청크 인벤토리를 별도로 관리합니다.
/// </summary>
public class PlayerInventory : Singleton<PlayerInventory>
{
    // --- 1. 인벤토리 데이터 ---

    [Header("Hotbar Inventory")]
    // 핫바 9칸에 대한 데이터 리스트
    public List<InventorySlot> hotbarSlots = new List<InventorySlot>(9);

    [Header("Chunk Inventory (GDD 4.5.1)")]
    // 획득한 청크 아이템을 저장하는 별도 인벤토리
    public List<ChunkItemData> chunkInventory = new List<ChunkItemData>();


    // --- 2. UI 갱신을 위한 이벤트 (GDD 4.7) ---

    /// <summary>
    /// 핫바의 아이템/수량이 변경될 때 UIManager에 알리기 위한 이벤트입니다.
    /// </summary>
    public event Action OnHotbarChanged;

    /// <summary>
    /// 청크 인벤토리의 내용이 변경될 때 UIManager에 알리기 위한 이벤트입니다.
    /// </summary>
    public event Action OnChunkInventoryChanged;


    // --- 3. Unity 생명주기 ---

    protected override void Awake()
    {
        base.Awake();
        // 핫바 슬롯 리스트를 9개의 빈 공간으로 초기화
        InitializeHotbar();
    }

    /// <summary>
    /// 핫바 리스트를 9개의 빈(null) 슬롯으로 채웁니다.
    /// </summary>
    void InitializeHotbar()
    {
        hotbarSlots.Clear();
        for (int i = 0; i < 9; i++)
        {
            hotbarSlots.Add(null); // 9개의 빈 슬롯(null)으로 채웁니다.
        }
    }

    // --- 4. 핫바 아이템 관리 ---

    /// <summary>
    /// (테스트 및 임시) 핫바에 아이템을 추가하는 간단한 함수입니다.
    /// </summary>
    public void AddItemToHotbar(ItemData item, int amount)
    {
        // TODO: 이미 아이템이 있는지 확인하고 수량만 더하는 로직 필요
        // (임시) 간단하게 첫 번째 빈 슬롯에 아이템 추가
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] == null || hotbarSlots[i].itemDataRef == null)
            {
                hotbarSlots[i] = new InventorySlot(item, amount);
                // ★ UI 갱신 이벤트 발생!
                OnHotbarChanged?.Invoke();
                Debug.Log($"[Inventory] 핫바 {i}번 슬롯에 {item.name} 추가");
                return;
            }
        }

        Debug.LogWarning($"[Inventory] 핫바가 가득 차서 {item.name}을(를) 추가할 수 없습니다.");
    }

    /// <summary>
    /// (8단계 Save/Load용) 핫바 데이터를 직렬화 가능한 형태로 반환합니다.
    /// </summary>
    public List<InventorySlot> GetHotbarSaveData()
    {
        // hotbarSlots는 이미 [System.Serializable]이므로 그대로 반환
        return hotbarSlots;
    }

    /// <summary>
    /// (8단계 Save/Load용) 핫바 데이터를 로드합니다.
    /// </summary>
    public void LoadHotbarData(List<InventorySlot> data)
    {
        hotbarSlots = data;
        OnHotbarChanged?.Invoke(); // UI 갱신
    }


    // --- 5. 청크 인벤토리 관리 (GDD 4.5.1) ---

    /// <summary>
    /// 청크 인벤토리에 획득한 청크를 추가합니다. (PuzzleManager가 호출)
    /// </summary>
    public void AddChunkItem(ChunkItemData chunk)
    {
        chunkInventory.Add(chunk);
        OnChunkInventoryChanged?.Invoke(); // 청크 UI 갱신 이벤트 발행
        Debug.Log($"[Inventory] 청크 인벤토리에 {chunk.name} 추가");
    }

    /// <summary>
    /// 청크 인벤토리에서 사용한 청크를 제거합니다. (PlayerBuildController가 호출)
    /// </summary>
    public void RemoveChunkItem(ChunkItemData chunk)
    {
        if (chunkInventory.Remove(chunk))
        {
            OnChunkInventoryChanged?.Invoke(); // 청크 UI 갱신 이벤트 발행
            Debug.Log($"[Inventory] 청크 인벤토리에서 {chunk.name} 사용/제거");
        }
    }

    /// <summary>
    /// (8단계 Save/Load용) 청크 인벤토리 데이터를 직렬화 가능한 형태로 반환합니다.
    /// </summary>
    public List<string> GetChunkInventorySaveData()
    {
        List<string> chunkIds = new List<string>();
        foreach (var chunk in chunkInventory)
        {
            // chunkIds.Add(chunk.id); // ChunkItemData에 'id' 필드가 있다고 가정
        }
        return chunkIds;
    }

    /// <summary>
    /// (8단계 Save/Load용) 청크 인벤토리 데이터를 로드합니다.
    /// </summary>
    public void LoadChunkInventoryData(List<string> data)
    {
        chunkInventory.Clear();
        foreach (var id in data)
        {
            // ChunkItemData item = DataManager.Instance.GetChunkData(id); // DataManager에 ID로 청크 데이터 찾는 기능 필요
            // if(item != null) chunkInventory.Add(item);
        }
        OnChunkInventoryChanged?.Invoke(); // UI 갱신
    }
}