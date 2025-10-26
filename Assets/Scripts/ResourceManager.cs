using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // FindResourceByID 사용 위해 추가

/// <summary>
/// 게임 내 실시간 자원(수량) 및 ScriptableObject 데이터 관리를 담당하는 싱글톤 매니저입니다.
/// ResourceData ScriptableObject를 사용합니다.
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    // --- ScriptableObject 데이터 관리 ---

    // 로드된 모든 ResourceData를 저장 (ID로 접근하기 위함)
    private Dictionary<string, ResourceData> resourceDataDict = new Dictionary<string, ResourceData>();
    // 모든 ResourceData 리스트 (반복문 등에서 사용)
    private List<ResourceData> allResourceData = new List<ResourceData>();

    // 다른 데이터 타입 (BlockData, ItemData 등)
    private Dictionary<string, BlockData> blockDataDict = new Dictionary<string, BlockData>();
    private Dictionary<string, ItemData> itemDataDict = new Dictionary<string, ItemData>();
    // ... RecipeDataDict 등 추가 ...


    // --- 실시간 자원 관리 ---

    // 자원 ID(string)를 키로, 현재 수량을 값으로 저장하는 딕셔너리
    private Dictionary<string, int> currentResources = new Dictionary<string, int>();

    /// <summary>
    /// 특정 자원의 수량이 변경될 때 발생하는 이벤트입니다. (ResourceData, 변경 후 총 수량)
    /// </summary>
    public event Action<ResourceData, int> OnResourceChanged;


    // --- Unity Lifecycle Methods ---

    protected override void Awake()
    {
        base.Awake(); // 싱글톤 초기화
        LoadAllGameData(); // 게임 시작 시 모든 SO 데이터 로드
        InitializeStartingResources(); // 초기 자원 설정
    }

    // --- ScriptableObject 데이터 로딩 ---

    private void LoadAllGameData()
    {
        // ResourceData 로드
        LoadDataFromResources<ResourceData>("Data/Resources", resourceDataDict, data => data.resourceID);
        allResourceData = resourceDataDict.Values.ToList(); // 리스트에도 저장

        // BlockData 로드
        LoadDataFromResources<BlockData>("Data/Blocks", blockDataDict, data => data.blockID); // BlockData에 blockID 필드 필요

        // ItemData 로드
        LoadDataFromResources<ItemData>("Data/Items", itemDataDict, data => data.itemID); // ItemData에 itemID 필드 필요

        // TODO: 다른 데이터 타입 로드

        Debug.Log($"게임 데이터 로딩 완료: Resources({resourceDataDict.Count}), Blocks({blockDataDict.Count}), Items({itemDataDict.Count})");
    }

    private void LoadDataFromResources<T>(string path, Dictionary<string, T> dictionary, Func<T, string> keySelector) where T : ScriptableObject
    {
        T[] loadedData = Resources.LoadAll<T>(path);
        foreach (T data in loadedData)
        {
            string key = keySelector(data);
            if (!string.IsNullOrEmpty(key) && !dictionary.ContainsKey(key))
            {
                dictionary.Add(key, data);
            }
            else
            {
                Debug.LogWarning($"데이터 로딩 중복 또는 키 오류: Type={typeof(T)}, Key={key}, AssetName={data.name}");
            }
        }
    }

    /// <summary>
    /// ID를 이용해 로드된 ResourceData를 가져옵니다.
    /// </summary>
    public ResourceData GetResourceData(string resourceID)
    {
        resourceDataDict.TryGetValue(resourceID, out ResourceData data);
        if (data == null) Debug.LogError($"ResourceData를 찾을 수 없습니다: ID={resourceID}");
        return data;
    }

    /// <summary>
    /// 모든 ResourceData 리스트를 반환합니다. (UI 표시 등)
    /// </summary>
    public List<ResourceData> GetAllResourceData()
    {
        return allResourceData;
    }

    // --- 다른 데이터 Get 메서드들 ---
    public BlockData GetBlockData(string blockID)
    {
        blockDataDict.TryGetValue(blockID, out BlockData data);
        if (data == null) Debug.LogError($"BlockData를 찾을 수 없습니다: ID={blockID}");
        return data;
    }

    public ItemData GetItemData(string itemID)
    {
        itemDataDict.TryGetValue(itemID, out ItemData data);
        if (data == null) Debug.LogError($"ItemData를 찾을 수 없습니다: ID={itemID}");
        return data;
    }
    // ...

    // --- 실시간 자원 관리 ---

    private void InitializeStartingResources()
    {
        // 로드된 모든 ResourceData에 대해 수량을 0으로 초기화
        foreach (var resourceData in allResourceData)
        {
            if (!currentResources.ContainsKey(resourceData.resourceID))
            {
                currentResources.Add(resourceData.resourceID, 0);
            }
        }

        // 초기 지급 자원 설정 (예시, ID 사용)
        AddResource("iron_ore", 50);
        AddResource("copper_ore", 30);
    }

    /// <summary>
    /// 특정 자원의 현재 보유량을 반환합니다.
    /// </summary>
    public int GetResourceAmount(string resourceID)
    {
        currentResources.TryGetValue(resourceID, out int amount);
        return amount;
    }

    /// <summary>
    /// 특정 자원의 현재 보유량을 반환합니다. (ResourceData 객체 사용)
    /// </summary>
    public int GetResourceAmount(ResourceData resourceData)
    {
        if (resourceData == null) return 0;
        return GetResourceAmount(resourceData.resourceID);
    }

    /// <summary>
    /// 특정 자원을 지정된 양만큼 추가합니다. (ID 사용)
    /// </summary>
    public void AddResource(string resourceID, int amount)
    {
        if (string.IsNullOrEmpty(resourceID) || amount <= 0 || !resourceDataDict.ContainsKey(resourceID)) return;

        if (currentResources.ContainsKey(resourceID))
        {
            currentResources[resourceID] += amount;
        }
        else
        {
            // 이 경우는 Initialize에서 이미 처리되었으므로 발생하면 안됨
            currentResources.Add(resourceID, amount);
            Debug.LogWarning($"{resourceID} 자원이 초기화되지 않았었습니다. 확인 필요.");
        }

        // 이벤트 호출 (ResourceData 객체 전달)
        OnResourceChanged?.Invoke(resourceDataDict[resourceID], currentResources[resourceID]);
        Debug.Log($"{resourceDataDict[resourceID].resourceName} 자원 {amount}개 추가. 현재: {currentResources[resourceID]}개");
    }

    /// <summary>
    /// 특정 자원을 지정된 양만큼 추가합니다. (ResourceData 객체 사용)
    /// </summary>
    public void AddResource(ResourceData resourceData, int amount)
    {
        if (resourceData == null) return;
        AddResource(resourceData.resourceID, amount);
    }

    /// <summary>
    /// 특정 자원을 지정된 양만큼 제거(소모)합니다. 성공하면 true, 부족하면 false를 반환합니다. (ID 사용)
    /// </summary>
    public bool RemoveResource(string resourceID, int amount)
    {
        if (string.IsNullOrEmpty(resourceID) || amount <= 0 || !resourceDataDict.ContainsKey(resourceID)) return false;

        if (currentResources.TryGetValue(resourceID, out int currentAmount))
        {
            if (currentAmount >= amount)
            {
                currentResources[resourceID] -= amount;
                // 이벤트 호출
                OnResourceChanged?.Invoke(resourceDataDict[resourceID], currentResources[resourceID]);
                Debug.Log($"{resourceDataDict[resourceID].resourceName} 자원 {amount}개 제거. 현재: {currentResources[resourceID]}개");
                return true; // 성공
            }
            else
            {
                Debug.LogWarning($"{resourceDataDict[resourceID].resourceName} 자원 부족! 필요: {amount}, 보유: {currentAmount}");
                return false; // 자원 부족
            }
        }
        else
        {
            Debug.LogWarning($"{resourceID} 자원이 없습니다 (초기화 오류?).");
            return false; // 해당 자원 없음 (오류 가능성)
        }
    }

    /// <summary>
    /// 특정 자원을 지정된 양만큼 제거(소모)합니다. (ResourceData 객체 사용)
    /// </summary>
    public bool RemoveResource(ResourceData resourceData, int amount)
    {
        if (resourceData == null) return false;
        return RemoveResource(resourceData.resourceID, amount);
    }

    /// <summary>
    /// 특정 자원을 지정된 양만큼 보유하고 있는지 확인합니다. (ID 사용)
    /// </summary>
    public bool HasEnoughResource(string resourceID, int amount)
    {
        if (string.IsNullOrEmpty(resourceID) || !resourceDataDict.ContainsKey(resourceID)) return false;
        if (amount <= 0) return true;

        return GetResourceAmount(resourceID) >= amount;
    }

    /// <summary>
    /// 특정 자원을 지정된 양만큼 보유하고 있는지 확인합니다. (ResourceData 객체 사용)
    /// </summary>
    public bool HasEnoughResource(ResourceData resourceData, int amount)
    {
        if (resourceData == null) return false;
        return HasEnoughResource(resourceData.resourceID, amount);
    }


    // --- Save/Load 연동 ---

    /// <summary>
    /// 현재 자원 상태를 저장용 데이터로 변환합니다. (ID 기반)
    /// </summary>
    public Dictionary<string, int> GetResourceSaveData()
    {
        return new Dictionary<string, int>(currentResources);
    }

    /// <summary>
    /// 저장된 데이터로부터 자원 상태를 복원합니다. (ID 기반)
    /// </summary>
    public void LoadResourceSaveData(Dictionary<string, int> loadedData)
    {
        if (loadedData == null) return;

        // 로드된 데이터로 덮어쓰기 (로드 시점에 존재하지 않는 자원 ID는 무시될 수 있음)
        currentResources = new Dictionary<string, int>(loadedData);

        // 로드되지 않은 자원 ID가 있다면 0으로 초기화 (게임 업데이트 등으로 자원이 추가된 경우)
        foreach (var resourceData in allResourceData)
        {
            if (!currentResources.ContainsKey(resourceData.resourceID))
            {
                currentResources.Add(resourceData.resourceID, 0);
            }
        }

        // 로드 후 모든 자원에 대해 이벤트 한번씩 호출 (UI 갱신 등)
        foreach (var pair in currentResources)
        {
            if (resourceDataDict.TryGetValue(pair.Key, out ResourceData data))
            {
                OnResourceChanged?.Invoke(data, pair.Value);
            }
        }
        Debug.Log("자원 데이터 로드 완료.");
    }
}