using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 월드의 3D 그리드 상에 블록 객체를 배치, 제거, 조회하는 싱글톤 매니저입니다.
/// </summary>
public class GridSystem : Singleton<GridSystem>
{
    // Vector3Int 좌표를 키로, 배치된 BlockObject를 값으로 가지는 딕셔너리
    private Dictionary<Vector3Int, BlockObject> blockGrid = new Dictionary<Vector3Int, BlockObject>();

    // 블록 오브젝트들을 관리할 부모 Transform (씬 정리를 위해)
    [SerializeField] private Transform blockParentTransform;

    protected override void Awake()
    {
        base.Awake();
        if (blockParentTransform == null)
        {
            // 부모 Transform이 지정되지 않았다면 "Blocks" 이름으로 하나 생성
            blockParentTransform = new GameObject("Blocks").transform;
            blockParentTransform.SetParent(this.transform);
        }
    }

    /// <summary>
    /// 지정된 위치에 블록을 배치합니다.
    /// </summary>
    /// <param name="blockData">배치할 블록의 데이터</param>
    /// <param name="position">배치할 그리드 좌표</param>
    /// <param name="rotation">배치할 블록의 회전값</param>
    /// <returns>배치 성공 여부</returns>
    public bool PlaceBlock(BlockData blockData, Vector3Int position, Quaternion rotation)
    {
        if (IsOccupied(position))
        {
            Debug.LogWarning($"Cannot place block at {position}: Already occupied.");
            return false;
        }


        if (blockData.prefab == null)
        {
            Debug.LogError($"BlockData '{blockData.name}' has no prefab assigned!");
            return false;
        }

        // 블록 프리팹 인스턴스화
        GameObject blockInstance = Instantiate(blockData.prefab, position, rotation, blockParentTransform);
        blockInstance.name = $"{blockData.blockName} ({position.x}, {position.y}, {position.z})";

        // BlockObject 컴포넌트 가져오기 (또는 추가)
        BlockObject blockObject = blockInstance.GetComponent<BlockObject>();
        if (blockObject == null)
        {
            // 만약 프리팹에 BlockObject가 없다면 추가 (하지만 프리팹에 미리 추가해두는 것이 좋음)
            Debug.LogWarning($"Prefab for '{blockData.name}' is missing BlockObject component. Adding it.", blockInstance);
            blockObject = blockInstance.AddComponent<BlockObject>();
        }

        // TODO: BlockObject 초기화 (데이터 및 상태 설정)
        // blockObject.Initialize(blockData, new BlockStateData()); [cite_start]// BlockStateData 구현 필요 [cite: 68]
        blockObject.Initialize(blockData); // 임시: 상태 없이 데이터만 전달


        blockGrid.Add(position, blockObject);

        Debug.Log($"Placed block '{blockData.blockName}' at {position}");
        return true;
    }

    /// <summary>
    /// 지정된 위치의 블록을 제거합니다.
    /// </summary>
    /// <param name="position">제거할 블록의 그리드 좌표</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveBlock(Vector3Int position)
    {
        if (blockGrid.TryGetValue(position, out BlockObject blockToRemove))
        {
            blockGrid.Remove(position);

            // 게임 오브젝트 파괴
            Destroy(blockToRemove.gameObject);

            Debug.Log($"Removed block at {position}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Cannot remove block at {position}: No block found.");
            return false;
        }
    }

    /// <summary>
    /// 지정된 위치에 있는 BlockObject를 반환합니다. 없으면 null을 반환합니다.
    /// </summary>
    /// <param name="position">조회할 그리드 좌표</param>
    /// <returns>해당 위치의 BlockObject 또는 null</returns>
    public BlockObject GetBlockAt(Vector3Int position)
    {
        blockGrid.TryGetValue(position, out BlockObject blockObject);
        return blockObject;
    }

    /// <summary>
    /// 지정된 위치가 점유되었는지 확인합니다.
    /// </summary>
    /// <param name="position">확인할 그리드 좌표</param>
    /// <returns>점유 여부</returns>
    public bool IsOccupied(Vector3Int position)
    {
        return blockGrid.ContainsKey(position);
    }

    /// <summary>
    /// 특정 청크 내의 모든 블록 정보를 반환합니다. (ChunkManager와 연동 필요)
    /// </summary>
    /// <param name="chunkCoord">청크 좌표</param>
    /// <returns>해당 청크 내 블록 리스트</returns>
    public List<BlockObject> GetBlocksInChunk(Vector3Int chunkCoord)
    {
        // TODO: ChunkManager에서 청크 크기(size)와 청크 월드 기준 좌표(chunkWorldPos)를 얻어와야 함
        int chunkSize = 16; // 예시 값, ChunkManager에서 가져와야 함
        Vector3 chunkWorldPos = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, chunkCoord.z * chunkSize); // 청크 좌표계에 따라 y 또는 z 사용

        List<BlockObject> blocksInChunk = new List<BlockObject>();
        foreach (var pair in blockGrid)
        {
            // 블록의 월드 좌표가 해당 청크 범위 내에 있는지 확인
            Vector3Int blockPos = pair.Key;
            // 예시: 2D 청크 기준 (XZ 평면)
            if (blockPos.x >= chunkWorldPos.x && blockPos.x < chunkWorldPos.x + chunkSize &&
                blockPos.z >= chunkWorldPos.z && blockPos.z < chunkWorldPos.z + chunkSize)
            {
                // TODO: 3D 청크 또는 Y축 레이어를 고려하는 경우 로직 수정 필요
                blocksInChunk.Add(pair.Value);
            }
        }
        return blocksInChunk;
    }

    // --- 저장/로드 관련 함수 ---

    /// <summary>
    /// 현재 그리드의 모든 블록 상태를 반환합니다. (저장 시 사용)
    /// </summary>
    public Dictionary<Vector3Int, BlockStateData> GetAllBlockStates()
    {
        Dictionary<Vector3Int, BlockStateData> allStates = new Dictionary<Vector3Int, BlockStateData>();
        foreach (var pair in blockGrid)
        {
             // BlockStateData state = pair.Value.GetStateData();
             // if (state != null)
             // {
             //    allStates.Add(pair.Key, state);
             // }
        }
        Debug.LogWarning("GetAllBlockStates needs implementation for BlockStateData retrieval.");
        return allStates; // 임시 반환
    }

    /// <summary>
    /// 저장된 데이터로부터 그리드 상태를 복원합니다. (로드 시 사용)
    /// </summary>
    public void RestoreAllBlocks(Dictionary<Vector3Int, BlockStateData> savedStates)
    {
        // 기존 블록 모두 제거
        ClearGrid();

        if (savedStates == null) return;

        foreach (var pair in savedStates)
        {
            Vector3Int position = pair.Key;
            BlockStateData state = pair.Value;

            // BlockStateData에 저장된 BlockData ID를 이용해 DataManager에서 BlockData 찾기
            // BlockData blockData = DataManager.Instance.GetBlockData(state.blockDataId); // BlockStateData에 ID 필드 필요

            // 임시: BlockData 로드가 안되므로 로직 스킵. 실제 구현 시 아래 주석 해제 필요.
            Debug.LogWarning("RestoreAllBlocks needs BlockData loading based on StateData. Skipping block restoration.");
            /*
            if (blockData != null)
            {
                // 블록 배치 (회전값은 StateData에 저장되어 있어야 함)
                // Quaternion rotation = state.rotation; // BlockStateData에 rotation 필드 필요
                Quaternion rotation = Quaternion.identity; // 임시

                PlaceBlock(blockData, position, rotation);

                // 배치된 BlockObject에 상태 복원
                BlockObject placedBlock = GetBlockAt(position);
                if (placedBlock != null)
                {
                     // TODO: BlockObject에 상태를 설정하는 기능 필요
                     // placedBlock.SetStateData(state);
                }
            }
            else
            {
                 Debug.LogError($"Could not restore block at {position}: BlockData with ID '{state.blockDataId}' not found.");
            }
            */
        }
        Debug.Log($"Restored {savedStates.Count} blocks.");
    }

    /// <summary>
    /// 그리드의 모든 블록을 제거합니다. (씬 로드 또는 게임 로드 시 사용)
    /// </summary>
    public void ClearGrid()
    {
        // Key 리스트를 복사해서 순회 (딕셔너리 변경 중 순회 오류 방지)
        List<Vector3Int> positions = new List<Vector3Int>(blockGrid.Keys);
        foreach (var pos in positions)
        {
            RemoveBlock(pos);
        }
        blockGrid.Clear();
        Debug.Log("Grid cleared.");
    }
}

// --- 필요한 보조 클래스 (별도 파일로 분리하는 것이 좋음) ---

/// <summary>
/// 월드에 배치되는 모든 블록의 기본 클래스 (MonoBehaviour)
/// </summary>
public class BlockObject : MonoBehaviour
{
    public BlockData Data { get; private set; }
    // public BlockStateData State { get; private set; [cite_start]} // 상태 데이터 [cite: 68]

    // 임시 초기화 함수 (상태 없이)
    public virtual void Initialize(BlockData data)
    {
        this.Data = data;
        // this.State = new BlockStateData(data.id); // 상태 데이터 초기화 (BlockStateData 구현 필요)
    }

    // TODO: 저장/로드 위한 상태 데이터 Get/Set 함수 구현
    // public BlockStateData GetStateData() { return State; }
    // public void SetStateData(BlockStateData newState) { State = newState; /* 상태에 따른 업데이트 로직 */}
}

/// <summary>
/// 블록의 동적 상태 데이터 (저장/로드 대상)
/// </summary>
[System.Serializable]
public class BlockStateData
{
    public string blockDataId; // 어떤 블록인지 식별하기 위한 ID (BlockData의 id 또는 name)
    public float rotationY; // 예시: Y축 회전값만 저장 (Quaternion 직접 직렬화는 복잡할 수 있음)
    // 필요한 다른 상태 변수들 추가 (예: 내구도, 내부 타이머, 인벤토리 내용 등)

    // TODO: 생성자, 상태 업데이트 메서드 등 필요에 따라 추가
    // 예시: ID를 받는 생성자
    public BlockStateData(string id)
    {
        blockDataId = id;
        rotationY = 0;
    }
}