using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 사용 추가

/// <summary>
/// 게임에 사용되는 모든 ScriptableObject 데이터를 로드하고 관리하는 싱글톤 매니저입니다.
/// </summary>
public class DataManager : Singleton<DataManager>
{
    // 각 데이터 타입을 ID(문자열)로 관리하기 위한 딕셔너리
    private Dictionary<string, BlockData> blockDatas = new Dictionary<string, BlockData>();
    private Dictionary<string, ItemData> itemDatas = new Dictionary<string, ItemData>();
    private Dictionary<string, ChunkItemData> chunkItemDatas = new Dictionary<string, ChunkItemData>();
    private Dictionary<string, RecipeData> recipeDatas = new Dictionary<string, RecipeData>();
    // --- ChunkRecipeData 딕셔너리 추가 ---
    private Dictionary<string, ChunkRecipeData> chunkRecipeDatas = new Dictionary<string, ChunkRecipeData>();
    // 필요에 따라 다른 데이터 타입(ResourceData, FluidData 등) 딕셔너리 추가
    private Dictionary<string, BuildingItemData> buildingItemDatas = new Dictionary<string, BuildingItemData>();

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    /// <summary>
    /// Resources 폴더 또는 지정된 경로에서 모든 ScriptableObject 데이터를 로드합니다.
    /// </summary>
    private void LoadAllData()
    {
        // 예시: Resources/Data/Blocks 폴더에서 BlockData 로드
        LoadData<BlockData>("Data/Blocks", blockDatas);
        LoadData<ItemData>("Data/Items", itemDatas); // ItemData 로드 경로 (예시)
        LoadData<ChunkItemData>("Data/ChunkItems", chunkItemDatas); // ChunkItemData 로드 경로 (예시)
        LoadData<RecipeData>("Data/Recipes", recipeDatas); // 일반 RecipeData 로드 경로 (예시)

        // --- ChunkRecipeData 로드 추가 ---
        // 경로 예시: Assets/Resources/Data/ChunkRecipes 폴더에 ChunkRecipeData 에셋들을 저장하세요.
        LoadData<ChunkRecipeData>("Data/ChunkRecipes", chunkRecipeDatas);

        LoadData<BuildingItemData>("Data/BuildingItems", buildingItemDatas);

        // 다른 데이터 로드 호출...

        Debug.Log("All game data loaded.");
    }

    /// <summary>
    /// 제네릭 메서드를 사용하여 특정 타입의 데이터를 로드하고 딕셔너리에 추가합니다.
    /// </summary>
    /// <typeparam name="T">ScriptableObject를 상속하는 데이터 타입</typeparam>
    /// <param name="path">Resources 내의 경로</param>
    /// <param name="dictionary">데이터를 저장할 딕셔너리</param>
    private void LoadData<T>(string path, Dictionary<string, T> dictionary) where T : ScriptableObject
    {
        var loadedData = Resources.LoadAll<T>(path);

        foreach (var data in loadedData)
        {
            // ScriptableObject의 name을 고유 ID(Key)로 사용
            // (별도 id 필드가 있다면 그것을 사용하는 것이 더 안전합니다: data.id)
            string dataId = data.name; // 또는 data.recipeId 등 고유 ID 필드
            if (!dictionary.ContainsKey(dataId))
            {
                dictionary.Add(dataId, data);
            }
            else
            {
                Debug.LogWarning($"Duplicate data key found: {dataId} in path {path}");
            }
        }
        Debug.Log($"Loaded {loadedData.Length} assets of type {typeof(T).Name} from Resources/{path}");
    }

    // --- 데이터 접근 함수들 ---

    public BlockData GetBlockData(string id)
    {
        if (blockDatas.TryGetValue(id, out BlockData data)) return data;
        Debug.LogError($"BlockData with ID '{id}' not found!");
        return null;
    }

    public ItemData GetItemData(string id)
    {
        if (itemDatas.TryGetValue(id, out ItemData data)) return data;
        if (chunkItemDatas.TryGetValue(id, out ChunkItemData chunkData)) return chunkData; // ChunkItemData도 ItemData임
        Debug.LogError($"ItemData (or ChunkItemData) with ID '{id}' not found!");
        return null;
    }

    public ChunkItemData GetChunkItemData(string id)
    {
        if (chunkItemDatas.TryGetValue(id, out ChunkItemData data)) return data;
        Debug.LogError($"ChunkItemData with ID '{id}' not found!");
        return null;
    }

    public RecipeData GetRecipeData(string id)
    {
        if (recipeDatas.TryGetValue(id, out RecipeData data)) return data;
        Debug.LogError($"RecipeData with ID '{id}' not found!");
        return null;
    }

    // --- ChunkRecipeData 접근 함수 추가 ---
    public ChunkRecipeData GetChunkRecipeData(string id)
    {
        if (chunkRecipeDatas.TryGetValue(id, out ChunkRecipeData data)) return data;
        Debug.LogError($"ChunkRecipeData with ID '{id}' not found!");
        return null;
    }

    public BuildingItemData GetBuildingItemData(string id)
    {
        if (buildingItemDatas.TryGetValue(id, out BuildingItemData data)) return data;
        Debug.LogError($"BuildingItemData with ID '{id}' not found!");
        return null;
    }

    // --- 전체 리스트 반환 함수들 ---

    public List<BlockData> GetAllBlockData()
    {
        return blockDatas.Values.ToList();
    }

    public List<RecipeData> GetAllRecipeData() // 일반 레시피 리스트 반환
    {
        return recipeDatas.Values.ToList();
    }

    // --- GetAllChunkRecipeData 함수 구현 ---
    /// <summary>
    /// 로드된 모든 ChunkRecipeData 리스트를 반환합니다. (PuzzleManager 등에서 사용)
    /// </summary>
    public List<ChunkRecipeData> GetAllChunkRecipeData()
    {
        return chunkRecipeDatas.Values.ToList();
    }

    // --- GetAllBuildingItemData 함수 추가 ---
    /// <summary>
    /// 로드된 모든 BuildingItemData 리스트를 반환합니다. (빌드 메뉴 UI용)
    /// </summary>
    public List<BuildingItemData> GetAllBuildingItemData()
    {
        return buildingItemDatas.Values.ToList();
    }

    // 다른 데이터 타입에 대한 접근 함수 및 전체 리스트 반환 함수 추가...
}