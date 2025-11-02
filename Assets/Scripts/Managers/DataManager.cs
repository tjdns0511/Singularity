// In Assets/Scripts/DataManager.cs

using System.Collections.Generic;
using System.Linq; // .ToList() 사용
using UnityEngine;

/// <summary>
/// GDD 4.4 - 게임의 모든 정적 데이터(ScriptableObject)를 로드하고 캐싱합니다.
/// 다른 모든 매니저들이 데이터에 접근할 수 있는 통로 역할을 합니다.
/// </summary>
public class DataManager : Singleton<DataManager>
{
    // --- 1. ScriptableObject 에셋 경로 (Resources 폴더 기준) ---
    // (이 경로는 실제 프로젝트 구조에 맞게 수정해야 합니다)
    private const string ITEM_DATA_PATH = "ScriptableObjects/Items";
    private const string BLOCK_DATA_PATH = "ScriptableObjects/Blocks";
    private const string CHUNK_ITEM_DATA_PATH = "ScriptableObjects/ChunkItems";
    private const string CHUNK_RECIPE_DATA_PATH = "ScriptableObjects/ChunkRecipes";

    // --- 2. 캐시된 데이터 딕셔너리 (GDD 4.4.1) ---
    // GDD 4.4.2 - ID를 Key로 사용하여 데이터에 빠르게 접근합니다.
    private Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    private Dictionary<string, BlockData> blockDataDic = new Dictionary<string, BlockData>();
    private Dictionary<string, ChunkItemData> chunkItemDataDic = new Dictionary<string, ChunkItemData>();
    private Dictionary<string, ChunkRecipeData> chunkRecipeDataDic = new Dictionary<string, ChunkRecipeData>();

    // --- 3. 초기화 ---

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    /// <summary>
    /// Resources 폴더에서 모든 SO 데이터를 로드하여 딕셔너리에 캐싱합니다.
    /// </summary>
    private void LoadAllData()
    {
        // GDD 4.4.1 - 모든 데이터 로드 및 캐싱
        LoadDataToDictionary(ITEM_DATA_PATH, itemDataDic);
        LoadDataToDictionary(BLOCK_DATA_PATH, blockDataDic);
        LoadDataToDictionary(CHUNK_ITEM_DATA_PATH, chunkItemDataDic);
        LoadDataToDictionary(CHUNK_RECIPE_DATA_PATH, chunkRecipeDataDic);

        Debug.Log($"[DataManager] 데이터 로드 완료: Items({itemDataDic.Count}), Blocks({blockDataDic.Count}), Recipes({chunkRecipeDataDic.Count})");
    }

    /// <summary>
    /// 제네릭을 사용한 공용 데이터 로드 함수
    /// (수정됨: data.id 필드 대신 data.ID 프로퍼티를 사용)
    /// </summary>
    private void LoadDataToDictionary<T>(string path, Dictionary<string, T> dictionary) where T : ScriptableObject, IDataWithId
    {
        T[] allData = Resources.LoadAll<T>(path);

        foreach (T data in allData)
        {
            // (수정) data.id가 아닌 data.ID (인터페이스 프로퍼티)를 사용합니다.
            if (string.IsNullOrEmpty(data.ID))
            {
                Debug.LogWarning($"[DataManager] ID가 없는 데이터 발견: {data.name} ({path})");
                continue;
            }

            // (수정) data.id가 아닌 data.ID (인터페이스 프로퍼티)를 사용합니다.
            if (!dictionary.ContainsKey(data.ID))
            {
                dictionary.Add(data.ID, data);
            }
            else
            {
                Debug.LogWarning($"[DataManager] 중복 ID 발견: {data.ID} ({path})");
            }
        }
    }

    // --- 4. 공용 데이터 접근 함수 (Getter) (GDD 4.4.2) ---

    // UIManager (빌드 메뉴)가 사용
    public List<BlockData> GetAllBlockData()
    {
        return blockDataDic.Values.ToList();
    }

    // PuzzleManager (조합)가 사용
    public List<ChunkRecipeData> GetAllChunkRecipes()
    {
        return chunkRecipeDataDic.Values.ToList();
    }

    // SaveLoadManager (블록 로드)가 사용
    public BlockData GetBlockData(string id)
    {
        if (blockDataDic.TryGetValue(id, out BlockData data))
        {
            return data;
        }
        Debug.LogError($"[DataManager] ID에 해당하는 BlockData를 찾을 수 없습니다: {id}");
        return null;
    }

    // PlayerInventory (핫바/아이템 로드)가 사용
    public ItemData GetItemData(string id)
    {
        if (itemDataDic.TryGetValue(id, out ItemData data))
        {
            return data;
        }
        Debug.LogError($"[DataManager] ID에 해당하는 ItemData를 찾을 수 없습니다: {id}");
        return null;
    }

    // PlayerInventory (청크 로드)가 사용
    public ChunkItemData GetChunkItemData(string id)
    {
        if (chunkItemDataDic.TryGetValue(id, out ChunkItemData data))
        {
            return data;
        }
        Debug.LogError($"[DataManager] ID에 해당하는 ChunkItemData를 찾을 수 없습니다: {id}");
        return null;
    }

    // PuzzleManager (조합 실패)가 사용
    public ItemData GetDefaultByproduct()
    {
        // (임시) 'junk'라는 ID를 가진 기본 부산물 아이템을 반환
        return GetItemData("byproduct_junk");
    }
}
// **적용 예시 (BlockData.cs):**
// public abstract class BlockData : ScriptableObject, IDataWithId
// {
//     [Header("Data ID")]
//     public string id; // 이 필드가 IDataWithId 인터페이스를 충족시킵니다.
//     string IDataWithId.id => id; // 인터페이스 명시적 구현
// 
//     // ... (displayName, icon, prefab 등 기존 변수) ...
// }