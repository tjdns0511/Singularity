// Description: 게임 ScriptableObject 데이터 로드 및 관리를 위한 싱글톤 매니저.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임 ScriptableObject 데이터 로드 및 접근 관리를 위한 싱글톤 클래스.
/// </summary>
public class DataManager : Singleton<DataManager>
{
    // 타입별 데이터 저장을 위한 Dictionary. Key는 SO 에셋 파일 이름(string).
    private Dictionary<string, BlockData> blockDataDict = new Dictionary<string, BlockData>();
    private Dictionary<string, ItemData> itemDataDict = new Dictionary<string, ItemData>();
    private Dictionary<string, ChunkItemData> chunkItemDataDict = new Dictionary<string, ChunkItemData>();
    private Dictionary<string, ChunkRecipeData> chunkRecipeDataDict = new Dictionary<string, ChunkRecipeData>();
    // TODO: 필요한 다른 SO 타입 Dictionary 추가 (FluidData, RecipeData 등)

    // Public 읽기 전용 접근자.
    public IReadOnlyDictionary<string, BlockData> BlockDatas => blockDataDict;
    public IReadOnlyDictionary<string, ItemData> ItemDatas => itemDataDict;
    public IReadOnlyDictionary<string, ChunkItemData> ChunkItemDatas => chunkItemDataDict;
    public IReadOnlyDictionary<string, ChunkRecipeData> ChunkRecipeDatas => chunkRecipeDataDict;

    /// <summary>
    /// 싱글톤 초기화 및 모든 데이터 로드를 위한 메서드.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    /// <summary>
    /// Resources 폴더 하위 모든 ScriptableObject 데이터 로드를 위한 메서드.
    /// </summary>
    private void LoadAllData()
    {
        LoadDataAtPath<BlockData>("Data/Blocks", blockDataDict);
        LoadDataAtPath<ItemData>("Data/Items", itemDataDict);
        LoadDataAtPath<ChunkRecipeData>("Data/ChunkRecipes", chunkRecipeDataDict);
        // TODO: 다른 SO 타입 로드 추가

        // 필요할 때 ItemDataDict에서 ChunkItemData만 분리하여 chunkItemDataDict 구성
        chunkItemDataDict = itemDataDict
            .Where(pair => pair.Value is ChunkItemData)
            .ToDictionary(pair => pair.Key, pair => pair.Value as ChunkItemData);

        Debug.Log($"DataManager loaded: {blockDataDict.Count} Blocks, {itemDataDict.Count} Items (incl. subtypes), {chunkItemDataDict.Count} ChunkItems, {chunkRecipeDataDict.Count} ChunkRecipes");
    }

    /// <summary>
    /// 지정된 경로에서 특정 타입 SO 에셋 로드 후 Dictionary에 추가하기 위한 제네릭 메서드.
    /// </summary>
    /// <typeparam name="T">로드할 SO 타입</typeparam>
    /// <param name="path">Resources 폴더 내 상대 경로</param>
    /// <param name="dictionary">데이터 저장할 Dictionary</param>
    private void LoadDataAtPath<T>(string path, Dictionary<string, T> dictionary) where T : ScriptableObject
    {
        var loadedAssets = Resources.LoadAll<T>(path);
        int count = 0;
        foreach (var asset in loadedAssets)
        {
            // ScriptableObject.name 은 에셋 파일 이름과 동일.
            if (!dictionary.ContainsKey(asset.name))
            {
                dictionary.Add(asset.name, asset);
                count++;
            }
        }
    }

    // --- 데이터 접근 함수 ---

    /// <summary>
    /// 이름(에셋 파일명)으로 BlockData 반환을 위한 메서드. 없으면 null 반환.
    /// </summary>
    public BlockData GetBlockData(string name)
    {
        blockDataDict.TryGetValue(name, out BlockData data);
        return data;
    }

    /// <summary>
    /// 이름(에셋 파일명)으로 ItemData (하위 타입 포함) 반환을 위한 메서드. 없으면 null 반환.
    /// </summary>
    public ItemData GetItemData(string name)
    {
        itemDataDict.TryGetValue(name, out ItemData data);
        return data;
    }

    /// <summary>
    /// 이름(에셋 파일명)으로 ChunkItemData 반환을 위한 메서드. 없으면 null 반환.
    /// </summary>
    public ChunkItemData GetChunkItemData(string name)
    {
        chunkItemDataDict.TryGetValue(name, out ChunkItemData data);
        return data;
    }

    /// <summary>
    /// 로드된 모든 ChunkRecipeData 리스트 반환을 위한 메서드. (PuzzleManager 등에서 사용)
    /// </summary>
    public List<ChunkRecipeData> GetAllChunkRecipeData()
    {
        return chunkRecipeDataDict.Values.ToList();
    }

    // TODO: 다른 SO 타입 접근 함수 추가 (GetFluidData, GetRecipeData 등)
}