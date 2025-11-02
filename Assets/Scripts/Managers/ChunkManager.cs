using System;
// Description: 게임 월드의 청크 생성, 배치, 제거, 관리를 위한 싱글톤 매니저.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 월드 청크 생성, 배치, 제거, 관리를 위한 싱글톤 클래스.
/// </summary>
public class ChunkManager : Singleton<ChunkManager>
{
    // 설치된 청크 좌표(XZ 평면 기준) 관리를 위한 HashSet.
    private HashSet<Vector2Int> placedChunkCoordinates = new HashSet<Vector2Int>();
    // 로드된 청크 객체 관리를 위한 Dictionary (월드 스트리밍 시 필요).
    private Dictionary<Vector2Int, ChunkObject> loadedChunks = new Dictionary<Vector2Int, ChunkObject>();

    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 16; // 청크 가로/세로 크기 (정사각형 가정).
    [SerializeField] private Transform chunkParentTransform; // 생성된 청크 오브젝트들의 부모 Transform.

    /// <summary>
    /// 외부에서 청크 크기를 읽기 위한 프로퍼티.
    /// </summary>
    public int ChunkSize => chunkSize;

    /// <summary>
    /// 싱글톤 초기화 및 청크 부모 Transform 설정을 위한 메서드.
    /// </summary>
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
    /// 지정된 위치에 청크 설치 가능 여부 확인을 위한 메서드. (다른 청크와 겹치는지 등)
    /// </summary>
    /// <param name="position">설치 시도 위치의 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>설치 가능 여부</returns>
    public bool IsPlacementValid(Vector3Int position)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(position);
        // 이미 해당 좌표에 청크가 있는지 확인
        if (placedChunkCoordinates.Contains(chunkCoord))
        {
            return false;
        }
        return true;
    }

    /// <summary>
        string chunkLabel = !string.IsNullOrWhiteSpace(chunkItemData.resourceName)
            ? chunkItemData.resourceName
            : (!string.IsNullOrWhiteSpace(chunkItemData.displayName)
                ? chunkItemData.displayName
                : chunkItemData.name);
        chunkInstance.name = $"Chunk ({chunkCoord.x}, {chunkCoord.y}) - {chunkLabel}";
        Debug.Log($"Placed chunk '{chunkLabel}' at coords {chunkCoord}");
    /// <param name="chunkItemData">설치할 청크 아이템 데이터</param>
    /// <param name="position">설치할 위치의 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>배치 성공 여부</returns>
    public bool PlaceChunk(ChunkItemData chunkItemData, Vector3Int position)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(position);

        if (!IsPlacementValid(position)) // 유효성 재확인
        {
            return false;
        }
        if (chunkItemData == null || chunkItemData.chunkPrefab == null)
        {
            Debug.LogError($"Cannot place chunk: Invalid ChunkItemData or missing chunkPrefab for '{chunkItemData?.name}'!");
            return false;
        }

        // 청크 기준 월드 위치 계산 및 인스턴스화
        Vector3 chunkWorldPosition = ChunkCoordsToWorldPosition(chunkCoord);
        GameObject chunkInstance = Instantiate(chunkItemData.chunkPrefab, chunkWorldPosition, Quaternion.identity, chunkParentTransform);
        chunkInstance.name = $"Chunk ({chunkCoord.x}, {chunkCoord.y}) - {chunkItemData.resourceName}";

        // ChunkObject 컴포넌트 가져오기 및 초기화
        ChunkObject chunkObject = chunkInstance.GetComponent<ChunkObject>();
        if (chunkObject == null)
        {
            // HACK: 프리팹에 ChunkObject가 없을 경우 동적 추가 (권장하지 않음).
            Debug.LogWarning($"Prefab for '{chunkItemData.name}' missing ChunkObject. Adding dynamically.", chunkInstance);
            chunkObject = chunkInstance.AddComponent<ChunkObject>();
        }
        chunkObject.Initialize(chunkCoord, chunkItemData); // TODO: 상태 데이터(ChunkStateData) 전달 로직 추가 필요.

        // 관리 목록에 추가
        placedChunkCoordinates.Add(chunkCoord);
        loadedChunks.Add(chunkCoord, chunkObject); // 로드된 청크 목록에도 추가

        Debug.Log($"Placed chunk '{chunkItemData.resourceName}' at coords {chunkCoord}");
        // TODO: 청크 배치 완료 이벤트 발행
        return true;
    }

    /// <summary>
    /// 지정된 위치의 청크 제거를 위한 메서드.
    /// </summary>
    /// <param name="position">제거할 청크가 있는 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveChunk(Vector3Int position)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(position);

        if (placedChunkCoordinates.Contains(chunkCoord))
        {
            // TODO: 청크 내 블록 제거 로직 필요 (GridSystem.GetBlocksInChunk / RemoveBlock 연동).
            // List<BlockObject> blocksInChunk = GridSystem.Instance.GetBlocksInChunk(chunkCoord);
            // foreach(var block in blocksInChunk) { GridSystem.Instance.RemoveBlock(Vector3Int.FloorToInt(block.transform.position)); }
            Debug.LogWarning("RemoveChunk needs implementation for removing blocks within the chunk via GridSystem.");

            // 관리 목록에서 제거
            placedChunkCoordinates.Remove(chunkCoord);
            if (loadedChunks.TryGetValue(chunkCoord, out ChunkObject chunkToRemove))
            {
                loadedChunks.Remove(chunkCoord);
                Destroy(chunkToRemove.gameObject); // 게임 오브젝트 파괴
            }

            Debug.Log($"Removed chunk at coords {chunkCoord}");
            // TODO: 청크 제거 완료 이벤트 발행
            return true;
        }
        else
        {
            Debug.LogWarning($"Cannot remove chunk at {position}: No chunk found at coords {chunkCoord}.");
            return false;
        }
    }

    /// <summary>
    /// 주어진 월드 좌표가 활성(설치된) 청크 내부에 있는지 확인하기 위한 메서드.
    /// </summary>
    /// <param name="worldPosition">확인할 월드 좌표 (Vector3)</param>
    /// <returns>활성 청크 내부 여부</returns>
    public bool IsPositionInActiveChunk(Vector3 worldPosition)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(worldPosition);
        return placedChunkCoordinates.Contains(chunkCoord);
    }
    /// <summary>
    /// 주어진 월드 좌표가 활성(설치된) 청크 내부에 있는지 확인하기 위한 메서드. (Vector3Int 오버로드)
    /// </summary>
    /// <param name="worldPosition">확인할 월드 그리드 좌표 (Vector3Int)</param>
    /// <returns>활성 청크 내부 여부</returns>
    public bool IsPositionInActiveChunk(Vector3Int worldPosition)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(worldPosition);
        return placedChunkCoordinates.Contains(chunkCoord);
    }

    /// <summary>
    /// 월드 좌표(Vector3)를 청크 좌표(Vector2Int, XZ 평면)로 변환하기 위한 메서드.
    /// </summary>
    public Vector2Int WorldToChunkCoords(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt(worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }
    /// <summary>
    /// 월드 그리드 좌표(Vector3Int)를 청크 좌표(Vector2Int, XZ 평면)로 변환하기 위한 메서드.
    /// </summary>
    public Vector2Int WorldToChunkCoords(Vector3Int worldPosition)
    {
        int x = Mathf.FloorToInt((float)worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt((float)worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 청크 좌표(Vector2Int)를 해당 청크의 월드 기준 위치(Vector3, Y=-16)로 변환하기 위한 메서드.
    /// </summary>
    public Vector3 ChunkCoordsToWorldPosition(Vector2Int chunkCoord)
    {
        return new Vector3(chunkCoord.x * chunkSize, -16, chunkCoord.y * chunkSize);
    }

    // --- 저장/로드 관련 ---

    /// <summary>
    /// 현재 배치된 모든 청크 정보 반환을 위한 메서드 (저장용).
    /// </summary>
    /// <returns>ChunkSaveData 리스트</returns>
    public List<ChunkSaveData> GetAllChunkSaveData()
    {
        List<ChunkSaveData> saveData = new List<ChunkSaveData>();
        foreach (var coord in placedChunkCoordinates)
        {
            if (loadedChunks.TryGetValue(coord, out ChunkObject chunkObj))
            {
                // TODO: ChunkObject.GetState() 구현 및 ChunkSaveData에 상태 저장 로직 필요.
                saveData.Add(new ChunkSaveData(coord, chunkObj.Data.name)); // 임시: ID만 저장
            }
        }
        Debug.LogWarning("GetAllChunkSaveData currently only saves coords & ID. ChunkStateData saving needed.");
        return saveData;
    }

    /// <summary>
    /// 저장된 데이터로부터 청크 상태 복원을 위한 메서드 (로드용).
    /// </summary>
    /// <param name="savedChunks">복원할 ChunkSaveData 리스트</param>
    public void RestoreAllChunks(List<ChunkSaveData> savedChunks)
    {
        ClearAllChunks(); // 복원 전 기존 청크 초기화
        if (savedChunks == null || savedChunks.Count == 0) return;
        Debug.Log($"Attempting to restore {savedChunks.Count} chunks...");

        foreach (var data in savedChunks)
        {
            // DataManager에서 ChunkItemData 찾기
            ChunkItemData chunkItemData = DataManager.Instance?.GetChunkItemData(data.chunkItemId);
            if (chunkItemData != null)
            {
                Vector3Int placementPos = Vector3Int.FloorToInt(ChunkCoordsToWorldPosition(data.chunkCoordinate));
                if (IsPlacementValid(placementPos)) // 배치 전 유효성 검사
                {
                    bool placed = PlaceChunk(chunkItemData, placementPos); // 청크 배치
                    if (placed && loadedChunks.TryGetValue(data.chunkCoordinate, out ChunkObject restoredChunk))
                    {
                        // TODO: restoredChunk?.SetState(data.chunkState) 호출 로직 구현 필요.
                        Debug.LogWarning($"Need SetState in ChunkObject for {chunkItemData.name} at {data.chunkCoordinate}");
                    }
                }
            }
            else
            {
                Debug.LogError($"ChunkItemData '{data.chunkItemId}' not found for restore at {data.chunkCoordinate}.");
            }
        }
        Debug.Log($"Finished restoring chunks. {placedChunkCoordinates.Count} chunks placed.");
    }

    /// <summary>
    /// 모든 청크 제거 및 초기화를 위한 메서드.
    /// </summary>
    public void ClearAllChunks()
    {
        if (placedChunkCoordinates.Count == 0 && loadedChunks.Count == 0) return; // 이미 비어있으면 스킵

        List<Vector2Int> coords = new List<Vector2Int>(placedChunkCoordinates); // 복사 후 순회
        foreach (var coord in coords)
        {
            RemoveChunk(Vector3Int.FloorToInt(ChunkCoordsToWorldPosition(coord))); // 임시 좌표 변환 사용
        }
        Debug.Log("All chunks cleared.");
    }
}


// --- 보조 클래스 ---
// NOTE: 별도 파일 분리 권장 (ChunkObject.cs, ChunkSaveData.cs, ChunkStateData.cs).

/// <summary>
/// 월드 배치 청크 프리팹 루트 컴포넌트용 클래스.
/// </summary>
public class ChunkObject : MonoBehaviour
{
    public Vector2Int Coordinate { get; private set; }
    public ChunkItemData Data { get; private set; }
    // TODO: 청크 고유 상태 데이터 (ChunkStateData State) 프로퍼티 추가 필요.

    /// <summary>
    /// 청크 오브젝트 초기화를 위한 메서드.
    /// </summary>
    public void Initialize(Vector2Int coordinate, ChunkItemData data)
    {
        this.Coordinate = coordinate;
        this.Data = data;
        // TODO: 상태 데이터(State) 객체 생성 및 초기화 로직.
    }

    // TODO: 저장/로드 위한 상태 Get/Set 가상 메서드 구현 필요 (GetState, SetState).
}

/// <summary>
/// 청크 저장용 직렬화 가능 데이터 구조체.
/// </summary>
[System.Serializable]
public class ChunkSaveData
{
    public Vector2Int chunkCoordinate;
    public string chunkItemId; // ChunkItemData 식별용 ID (name 등).
    // TODO: 청크 고유 상태 데이터 (ChunkStateData chunkState) 필드 추가 필요.

    // 임시 생성자 (상태 없이 ID만)
    public ChunkSaveData(Vector2Int coord, string itemId)
    {
        chunkCoordinate = coord;
        chunkItemId = itemId;
    }
}

/// <summary>
/// [미구현] 청크 고유 상태 저장/로드용 직렬화 가능 데이터 클래스 (필요시 정의).
/// </summary>
[System.Serializable]
public class ChunkStateData
{
    // TODO: 필요한 상태 변수 정의 (예: 업그레이드 레벨, 특수 효과 활성 여부).
    // public int upgradeLevel;
    // public bool isSpecialEffectActive;
}