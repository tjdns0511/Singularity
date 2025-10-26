// Description: 월드 그리드 상 블록 객체(BlockObject) 배치, 제거, 조회를 위한 싱글톤 매니저.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 월드 3D 그리드 시스템 관리를 위한 싱글톤 클래스.
/// 블록 객체(BlockObject) 배치, 제거, 조회 기능 제공.
/// </summary>
public class GridSystem : Singleton<GridSystem>
{
    // 그리드 좌표별 배치된 BlockObject 저장을 위한 Dictionary.
    private Dictionary<Vector3Int, BlockObject> blockGrid = new Dictionary<Vector3Int, BlockObject>();

    // 생성된 블록들을 묶을 부모 Transform (씬 정리용).
    [SerializeField] private Transform blockParentTransform;

    /// <summary>
    /// 싱글톤 초기화 및 블록 부모 Transform 설정을 위한 메서드.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (blockParentTransform == null)
        {
            GameObject blockParentObj = new GameObject("Blocks");
            blockParentTransform = blockParentObj.transform;
            blockParentTransform.SetParent(this.transform);
        }
    }

    /// <summary>
    /// 지정된 그리드 좌표에 블록 배치를 위한 메서드.
    /// </summary>
    /// <param name="blockData">배치할 블록 정보</param>
    /// <param name="position">배치할 그리드 좌표</param>
    /// <param name="rotation">배치할 블록 회전값</param>
    /// <returns>배치 성공 여부</returns>
    public bool PlaceBlock(BlockData blockData, Vector3Int position, Quaternion rotation)
    {
        // 배치 유효성 검사 (점유 여부, 데이터 유효성)
        if (IsOccupied(position))
        {
            Debug.LogWarning($"Cannot place block at {position}: Already occupied.");
            return false;
        }
        if (blockData == null || blockData.prefab == null)
        {
            Debug.LogError($"Cannot place block at {position}: Invalid BlockData or missing prefab.");
            return false;
        }

        // 블록 인스턴스화 및 이름 설정
        GameObject blockInstance = Instantiate(blockData.prefab, position, rotation, blockParentTransform);
        blockInstance.name = $"{blockData.blockName} ({position.x}, {position.y}, {position.z})";

        // BlockObject 컴포넌트 가져오기 및 초기화
        BlockObject blockObject = blockInstance.GetComponent<BlockObject>();
        if (blockObject == null)
        {
            Debug.LogWarning($"Prefab for '{blockData.name}' missing BlockObject. Adding dynamically.", blockInstance);
            blockObject = blockInstance.AddComponent<BlockObject>();
        }
        blockObject.Initialize(blockData); // TODO: 상태 데이터(BlockStateData) 전달 로직 추가 필요.

        // 그리드 데이터에 추가
        blockGrid[position] = blockObject;
        Debug.Log($"Placed block '{blockData.blockName}' at {position}");
        // TODO: 블록 배치 완료 이벤트 발행 (EventManager)
        return true;
    }

    /// <summary>
    /// 지정된 그리드 좌표의 블록 제거를 위한 메서드.
    /// </summary>
    /// <param name="position">제거할 블록의 그리드 좌표</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveBlock(Vector3Int position)
    {
        if (blockGrid.TryGetValue(position, out BlockObject blockToRemove))
        {
            blockGrid.Remove(position);
            Destroy(blockToRemove.gameObject); // 게임 오브젝트 파괴
            Debug.Log($"Removed block at {position}");
            // TODO: 블록 제거 완료 이벤트 발행 (EventManager)
            return true;
        }
        else
        {
            Debug.LogWarning($"Cannot remove block at {position}: No block found.");
            return false;
        }
    }

    /// <summary>
    /// 지정된 그리드 좌표의 BlockObject를 반환하기 위한 메서드. 없으면 null 반환.
    /// </summary>
    /// <param name="position">조회할 그리드 좌표</param>
    /// <returns>BlockObject 또는 null</returns>
    public BlockObject GetBlockAt(Vector3Int position)
    {
        blockGrid.TryGetValue(position, out BlockObject blockObject);
        return blockObject;
    }

    /// <summary>
    /// 지정된 그리드 좌표 점유 여부 확인을 위한 메서드.
    /// </summary>
    /// <param name="position">확인할 그리드 좌표</param>
    /// <returns>점유 여부</returns>
    public bool IsOccupied(Vector3Int position)
    {
        return blockGrid.ContainsKey(position);
    }

    /// <summary>
    /// [미구현] 특정 청크 내 모든 블록 객체 리스트 반환을 위한 메서드.
    /// </summary>
    /// <param name="chunkCoord">블록을 조회할 청크 좌표 (ChunkManager 좌표계와 일치 필요)</param>
    /// <returns>해당 청크 내 BlockObject 리스트</returns>
    public List<BlockObject> GetBlocksInChunk(Vector3Int chunkCoord)
    {
        // TODO: ChunkManager 연동 및 정확한 범위 계산 로직 구현 필요.
        int chunkSize = ChunkManager.Instance?.ChunkSize ?? 16;
        Vector3 chunkWorldOrigin = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, chunkCoord.z * chunkSize); // HACK: 임시 좌표 계산
        List<BlockObject> blocksInChunk = new List<BlockObject>();
        // TODO: 범위 체크 로직 추가 예정
        return blocksInChunk;
    }

    // --- 저장/로드 관련 ---

    /// <summary>
    /// 현재 그리드의 모든 블록 상태 데이터 반환을 위한 메서드 (저장용).
    /// </summary>
    /// <returns>좌표-상태 데이터 Dictionary</returns>
    public Dictionary<Vector3Int, BlockStateData> GetAllBlockStates()
    {
        Dictionary<Vector3Int, BlockStateData> allStates = new Dictionary<Vector3Int, BlockStateData>();
        // TODO: BlockObject.GetStateData() 구현 및 호출 로직 필요.
        Debug.LogWarning("GetAllBlockStates needs implementation. Returning empty dictionary.");
        return allStates;
    }

    /// <summary>
    /// 저장된 데이터로부터 그리드 상태 복원을 위한 메서드 (로드용).
    /// </summary>
    /// <param name="savedStates">복원할 좌표-상태 데이터 Dictionary</param>
    public void RestoreAllBlocks(Dictionary<Vector3Int, BlockStateData> savedStates)
    {
        ClearGrid(); // 복원 전 기존 그리드 초기화
        if (savedStates == null || savedStates.Count == 0) return;
        Debug.Log($"Attempting to restore {savedStates.Count} blocks...");

        foreach (var pair in savedStates)
        {
            Vector3Int position = pair.Key;
            BlockStateData state = pair.Value;

            if (state == null || string.IsNullOrEmpty(state.blockDataId)) continue;

            // DataManager에서 BlockData 찾기
            BlockData blockData = DataManager.Instance?.GetBlockData(state.blockDataId);
            if (blockData != null)
            {
                Quaternion rotation = Quaternion.Euler(0, state.rotationY, 0); // 상태에서 회전값 가져오기 (예시)
                bool placed = PlaceBlock(blockData, position, rotation); // 블록 배치

                if (placed)
                {
                    BlockObject placedBlock = GetBlockAt(position);
                    // TODO: placedBlock?.SetStateData(state) 호출 로직 구현 필요.
                    if (placedBlock == null) Debug.LogError($"Failed to get placed block at {position} after PlaceBlock returned true.");
                    else Debug.LogWarning($"Need SetStateData in BlockObject for {blockData.name} at {position}");
                }
            }
            else
            {
                Debug.LogError($"BlockData '{state.blockDataId}' not found for restore at {position}.");
            }
        }
        Debug.Log($"Finished restoring blocks. {blockGrid.Count} blocks in grid.");
    }

    /// <summary>
    /// 그리드의 모든 블록 제거 및 초기화를 위한 메서드.
    /// </summary>
    public void ClearGrid()
    {
        if (blockGrid.Count == 0) return; // 이미 비어있으면 스킵

        List<Vector3Int> positions = new List<Vector3Int>(blockGrid.Keys);
        foreach (var pos in positions)
        {
            RemoveBlock(pos); // 내부에서 Destroy 호출
        }

        Debug.Log("Grid cleared.");
    }
}


// --- 보조 클래스 ---
// NOTE: 별도 파일 분리 권장 (BlockObject.cs, BlockStateData.cs).

/// <summary>
/// 월드 배치 블록 오브젝트의 기본 MonoBehaviour 클래스.
/// </summary>
public class BlockObject : MonoBehaviour
{
    /// <summary>
    /// 블록의 정적 정보 참조용 프로퍼티.
    /// </summary>
    public BlockData Data { get; private set; }
    // TODO: 동적 상태 데이터 (BlockStateData State) 프로퍼티 추가 필요.

    /// <summary>
    /// 블록 오브젝트 초기화를 위한 가상 메서드.
    /// </summary>
    /// <param name="data">블록 데이터</param>
    public virtual void Initialize(BlockData data)
    {
        this.Data = data;
        // TODO: 상태 데이터(State) 객체 생성 및 초기화 로직.
    }

    // TODO: 저장/로드 위한 상태 Get/Set 가상 메서드 구현 필요 (GetStateData, SetStateData).
}

/// <summary>
/// 블록 동적 상태 저장/로드를 위한 직렬화 가능 데이터 클래스.
/// </summary>
[System.Serializable]
public class BlockStateData
{
    /// <summary>
    /// 블록 종류 식별용 ID (BlockData.name 등).
    /// </summary>
    public string blockDataId;
    /// <summary>
    /// 블록 Y축 회전값 (예시).
    /// </summary>
    public float rotationY;
    // TODO: 필요한 다른 상태 변수 추가 (인벤토리, 타이머 등).

    public BlockStateData() { } // 기본 생성자

    public BlockStateData(string id) // ID 받는 생성자 (예시)
    {
        blockDataId = id;
        rotationY = 0;
        // TODO: 다른 상태 변수 기본값 초기화.
    }
}