using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 월드의 청크 생성, 배치, 제거, 관리를 담당하는 싱글톤 매니저입니다.
/// </summary>
public class ChunkManager : Singleton<ChunkManager>
{
    // 설치된 청크의 좌표(XZ 평면 기준)를 관리하는 HashSet
    private HashSet<Vector2Int> placedChunkCoordinates = new HashSet<Vector2Int>();

    // 로드된 청크 객체를 관리하는 딕셔너리 (월드 스트리밍 시 필요)
    private Dictionary<Vector2Int, ChunkObject> loadedChunks = new Dictionary<Vector2Int, ChunkObject>();

    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 16; // 청크의 가로, 세로 크기 (정사각형 가정)
    [SerializeField] private Transform chunkParentTransform; // 생성된 청크 오브젝트들의 부모

    public int ChunkSize => chunkSize; // 외부에서 청크 크기를 읽을 수 있도록 프로퍼티 추가

    protected override void Awake()
    {
        base.Awake();
        if (chunkParentTransform == null)
        {
            chunkParentTransform = new GameObject("Chunks").transform;
            chunkParentTransform.SetParent(this.transform);
        }
    }

    /// <summary>
    /// 지정된 위치에 청크 설치가 가능한지 확인합니다. (다른 청크와 겹치는지 등)
    /// </summary>
    /// <param name="position">설치하려는 위치의 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>설치 가능 여부</returns>
    public bool IsPlacementValid(Vector3Int position)
    {
        // Vector3Int 월드 좌표를 청크 좌표(Vector2Int)로 변환
        Vector2Int chunkCoord = WorldToChunkCoords(position);

        // 이미 해당 좌표에 청크가 있는지 확인
        if (placedChunkCoordinates.Contains(chunkCoord))
        {
            // Debug.LogWarning($"Cannot place chunk at {chunkCoord}: Already occupied.");
            return false;
        }


        return true;
    }

    /// <summary>
    /// 지정된 위치에 청크를 배치합니다.
    /// </summary>
    /// <param name="chunkItemData">설치할 청크 아이템 데이터</param>
    /// <param name="position">설치할 위치의 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>배치 성공 여부</returns>
    public bool PlaceChunk(ChunkItemData chunkItemData, Vector3Int position)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(position);

        if (!IsPlacementValid(position)) // IsPlacementValid는 내부적으로 chunkCoord 변환 포함
        {
            Debug.LogWarning($"Placement validation failed again for chunk at {chunkCoord}. Aborting.");
            return false;
        }

        if (chunkItemData.chunkPrefab == null)
        {
            Debug.LogError($"ChunkItemData '{chunkItemData.name}' has no chunkPrefab assigned!");
            return false;
        }

        // 청크의 기준 위치 (예: 청크의 월드 (0,0,0) 좌표) 계산
        Vector3 chunkWorldPosition = ChunkCoordsToWorldPosition(chunkCoord);

        GameObject chunkInstance = Instantiate(chunkItemData.chunkPrefab, chunkWorldPosition, Quaternion.identity, chunkParentTransform);
        chunkInstance.name = $"Chunk ({chunkCoord.x}, {chunkCoord.y}) - {chunkItemData.resourceName}";

        ChunkObject chunkObject = chunkInstance.GetComponent<ChunkObject>();
        if (chunkObject == null)
        {
            Debug.LogWarning($"Prefab for '{chunkItemData.name}' is missing ChunkObject component. Adding it.", chunkInstance);
            chunkObject = chunkInstance.AddComponent<ChunkObject>();
        }

        // ChunkObject 초기화
        chunkObject.Initialize(chunkCoord, chunkItemData); 

        // 관리 목록에 추가
        placedChunkCoordinates.Add(chunkCoord); 
        loadedChunks.Add(chunkCoord, chunkObject); // 로드된 청크 목록에 추가


        Debug.Log($"Placed chunk '{chunkItemData.resourceName}' at coordinates {chunkCoord} (World Pos: {chunkWorldPosition})");
        return true;
    }

    /// <summary>
    /// 지정된 위치의 청크를 제거합니다.
    /// </summary>
    /// <param name="position">제거할 청크가 있는 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveChunk(Vector3Int position)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(position);

        if (placedChunkCoordinates.Contains(chunkCoord))
        {
            // List<BlockObject> blocksInChunk = GridSystem.Instance.GetBlocksInChunk(chunkCoord); // GridSystem 연동 필요
            // foreach(var block in blocksInChunk) { GridSystem.Instance.RemoveBlock(Vector3Int.FloorToInt(block.transform.position)); }
            Debug.LogWarning("RemoveChunk needs implementation for removing blocks within the chunk via GridSystem.");


            // 관리 목록에서 제거
            placedChunkCoordinates.Remove(chunkCoord);

            if (loadedChunks.TryGetValue(chunkCoord, out ChunkObject chunkToRemove))
            {
                loadedChunks.Remove(chunkCoord);
                Destroy(chunkToRemove.gameObject);
            }

            // TODO: 월드 데이터 변경 이벤트 발생
            // EventManager.Instance?.Publish(new OnChunkRemovedEvent(chunkCoord));

            Debug.Log($"Removed chunk at coordinates {chunkCoord}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Cannot remove chunk at {position}: No chunk found at coordinates {chunkCoord}.");
            return false;
        }
    }

    /// <summary>
    /// 주어진 월드 좌표가 현재 활성화된(설치된) 청크 내부에 있는지 확인합니다.
    /// (구버전 PlayerBuildController에서 사용하던 방식)
    /// </summary>
    /// <param name="worldPosition">월드 좌표 (Vector3 또는 Vector3Int)</param>
    /// <returns>활성 청크 내부 여부</returns>
    public bool IsPositionInActiveChunk(Vector3 worldPosition)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(worldPosition);
        return placedChunkCoordinates.Contains(chunkCoord);
    }
    public bool IsPositionInActiveChunk(Vector3Int worldPosition)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(worldPosition);
        return placedChunkCoordinates.Contains(chunkCoord);
    }


    /// <summary>
    /// 월드 좌표(Vector3)를 청크 좌표(Vector2Int, XZ 평면)로 변환합니다.
    /// </summary>
    public Vector2Int WorldToChunkCoords(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt(worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 월드 그리드 좌표(Vector3Int)를 청크 좌표(Vector2Int, XZ 평면)로 변환합니다.
    /// </summary>
    public Vector2Int WorldToChunkCoords(Vector3Int worldPosition)
    {
        // Vector3Int는 이미 정수 단위이므로 FloorToInt 필요 없음
        int x = Mathf.FloorToInt((float)worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt((float)worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
<<<<<<< HEAD:Assets/Scripts/Managers/ChunkManager.cs
    /// 청크 좌표(Vector2Int)를 해당 청크의 월드 기준 위치(Vector3, Y=-16)로 변환하기 위한 메서드.
=======
    /// 청크 좌표(Vector2Int)를 해당 청크의 월드 기준 위치(Vector3, 보통 Y=0)로 변환합니다.
>>>>>>> parent of cc42e85 (update):Assets/Scripts/ChunkManager.cs
    /// </summary>
    public Vector3 ChunkCoordsToWorldPosition(Vector2Int chunkCoord)
    {
        return new Vector3(chunkCoord.x * chunkSize, -16, chunkCoord.y * chunkSize);
    }

    // --- 저장/로드 관련 함수 ---

    /// <summary>
    /// 현재 배치된 모든 청크 정보를 반환합니다. (저장 시 사용)
    /// </summary>
    public List<ChunkSaveData> GetAllChunkSaveData()
    {
        List<ChunkSaveData> saveData = new List<ChunkSaveData>();
        foreach (var coord in placedChunkCoordinates)
        {
            if (loadedChunks.TryGetValue(coord, out ChunkObject chunkObj))
            {
                // ChunkObject에서 필요한 데이터 (어떤 ChunkItemData인지 ID, 청크 고유 상태 등) 가져오기
                // saveData.Add(new ChunkSaveData(coord, chunkObj.Data.id, chunkObj.GetState())); // ChunkObject에 GetState 구현 필요
                saveData.Add(new ChunkSaveData(coord, chunkObj.Data.name)); // 임시: ID만 저장
            }
            else
            {
                // 로드되지 않은 청크 처리 (필요시) - MVP에서는 모든 청크가 로드되어 있다고 가정
                Debug.LogWarning($"Chunk at {coord} is placed but not loaded. Cannot save its state.");
            }
        }
        Debug.LogWarning("GetAllChunkSaveData currently only saves coordinates and item ID. ChunkStateData saving needs implementation.");
        return saveData;
    }

    /// <summary>
    /// 저장된 데이터로부터 청크 상태를 복원합니다. (로드 시 사용)
    /// </summary>
    public void RestoreAllChunks(List<ChunkSaveData> savedChunks)
    {
        // 기존 청크 모두 제거
        ClearAllChunks();

        if (savedChunks == null) return;

        foreach (var data in savedChunks)
        {
            // ChunkItemData ID로 DataManager에서 데이터 찾기
            ChunkItemData chunkItemData = DataManager.Instance.GetChunkItemData(data.chunkItemId);
            if (chunkItemData != null)
            {
                // 청크 배치 (PlaceChunk는 내부적으로 좌표를 계산하므로, 복원 시에는 좌표 직접 사용 고려)
                // 임시로 PlaceChunk 사용, 단 좌표 변환 주의
                Vector3Int tempWorldPos = Vector3Int.FloorToInt(ChunkCoordsToWorldPosition(data.chunkCoordinate));
                bool placed = PlaceChunk(chunkItemData, tempWorldPos);

                if (placed && loadedChunks.TryGetValue(data.chunkCoordinate, out ChunkObject restoredChunk))
                {
                    // TODO: 청크 상태 복원 (ChunkObject에 SetState 구현 필요)
                    // restoredChunk.SetState(data.chunkState); // ChunkSaveData에 ChunkStateData 포함 필요
                }
            }
            else
            {
                Debug.LogError($"Could not restore chunk at {data.chunkCoordinate}: ChunkItemData with ID '{data.chunkItemId}' not found.");
            }
        }
        Debug.Log($"Restored {savedChunks.Count} chunks.");
    }

    /// <summary>
    /// 모든 청크를 제거합니다.
    /// </summary>
    public void ClearAllChunks()
    {
        // Key 리스트 복사 (딕셔너리 변경 중 순회 오류 방지)
        List<Vector2Int> coords = new List<Vector2Int>(placedChunkCoordinates);
        foreach (var coord in coords)
        {
            RemoveChunk(Vector3Int.FloorToInt(ChunkCoordsToWorldPosition(coord))); // 임시 좌표 변환
        }
        placedChunkCoordinates.Clear();
        loadedChunks.Clear();
        Debug.Log("All chunks cleared.");
    }
}


// --- 필요한 보조 클래스 (별도 파일로 분리하는 것이 좋음) ---

/// <summary>
/// 월드에 배치되는 청크 프리팹의 루트 컴포넌트
/// </summary>
public class ChunkObject : MonoBehaviour
{
    public Vector2Int Coordinate { get; private set; }
    public ChunkItemData Data { get; private set; }
    // public ChunkStateData State { get; private set; [cite_start]} // 청크 고유 상태 데이터 [cite: 15]

    public void Initialize(Vector2Int coordinate, ChunkItemData data)
    {
        this.Coordinate = coordinate;
        this.Data = data;
        // this.State = new ChunkStateData(); [cite_start]// 상태 데이터 초기화 (ChunkStateData 구현 필요) [cite: 15]
    }


    // TODO: 저장/로드 위한 상태 데이터 Get/Set 함수 구현
    // public ChunkStateData GetState() { return State; }
    // public void SetState(ChunkStateData newState) { State = newState; /* 상태 업데이트 로직 */ }
}

/// <summary>
/// 청크 저장을 위한 데이터 구조
/// </summary>
[System.Serializable]
public class ChunkSaveData
{
    public Vector2Int chunkCoordinate;
    public string chunkItemId; // 어떤 종류의 청크인지 식별 (ChunkItemData의 ID)
    // public ChunkStateData chunkState; [cite_start]// 청크의 고유 상태 (ChunkStateData 구현 필요) [cite: 15]

    // 임시 생성자 (상태 없이)
    public ChunkSaveData(Vector2Int coord, string itemId)
    {
        chunkCoordinate = coord;
        chunkItemId = itemId;
    }
}

/// <summary>
/// 청크의 고유 상태 데이터 (필요 시 정의)
/// </summary>
[System.Serializable]
public class ChunkStateData
{
    public int upgradeLevel;
    public bool isSpecialEffectActive;

    // TODO: 생성자, 상태 업데이트 메서드 등 필요에 따라 추가
}