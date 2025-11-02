// In Assets/Scripts/GridSystem.cs

using System.Collections.Generic;
using System.Linq; // .ToList() 사용
using UnityEngine;

/// <summary>
/// GDD 4.1 - 월드의 모든 블록 배치를 관리하는 중앙 시스템입니다.
/// 블록의 설치, 제거, 조회, 저장을 담당합니다.
/// </summary>
public class GridSystem : Singleton<GridSystem>
{
    [Header("Grid Settings")]
    [Tooltip("그리드 셀 하나의 크기 (보통 1)")]
    public float gridCellSize = 1f;

    [Tooltip("그리드의 월드 원점 좌표")]
    public Vector3 gridOrigin = Vector3.zero;

    /// <summary>
    /// GDD 4.1.1 - 현재 월드에 설치된 모든 블록을 저장하는 핵심 딕셔너리입니다.
    /// </summary>
    private Dictionary<Vector3Int, BlockObject> blockGrid = new Dictionary<Vector3Int, BlockObject>();

    // --- 1. 핵심 기능 (설치/제거/조회) ---

    /// <summary>
    /// GDD 4.1.2 - 지정된 좌표에 블록 설치를 시도합니다.
    /// PlayerBuildController가 호출합니다.
    /// </summary>
    public bool PlaceBlock(BlockData data, Vector3Int position, Quaternion rotation)
    {
        if (data == null)
        {
            Debug.LogError("[GridSystem] 설치할 BlockData가 null입니다.");
            return false;
        }

        // GDD 4.1.2 - 유효성 검사
        if (IsOccupied(position))
        {
            Debug.Log($"[GridSystem] {position} 위치는 이미 사용 중입니다.");
            return false;
        }

        // 1. GDD 4.2.1 - DataManager의 'prefab'을 기반으로 월드에 생성
        Vector3 worldPos = GridToWorldPosition(position);
        GameObject blockInstance = Instantiate(data.prefab, worldPos, rotation);
        blockInstance.name = $"{data.displayName}_{position}";

        // 2. BlockObject 컴포넌트 가져오기 (모든 블록 프리팹에 있어야 함)
        BlockObject blockObject = blockInstance.GetComponent<BlockObject>();
        if (blockObject == null)
        {
            Debug.LogError($"[GridSystem] {data.name} 프리팹에 BlockObject.cs 컴포넌트가 없습니다!");
            Destroy(blockInstance);
            return false;
        }

        // 3. GDD 4.2.1 - 새 블록의 정적/동적 데이터 설정
        blockObject.data = data;

        // (GDD 4.2.2) MachineData인 경우 MachineStateData로 생성
        // (7단계) if (data is MachineData)
        // {
        //     blockObject.state = new MachineStateData(); 
        // }
        // else
        // {
        //     blockObject.state = new BlockStateData();
        // }

        // (임시) 기본 BlockStateData 생성
        blockObject.state = new BlockStateData();

        blockObject.state.dataId = data.ID; // GDD 4.8.2 - 저장/로드를 위한 ID 기록
        blockObject.state.position = position;
        blockObject.state.rotation = rotation;


        // 4. GDD 4.1.1 - 그리드 딕셔너리에 등록
        blockGrid.Add(position, blockObject);

        // GDD 4.1.2 - (VFX/SFX)
        // VFXManager.Instance.PlayEffect("BlockPlace", worldPos);
        // SoundManager.Instance.PlaySound("BlockPlace");

        // GDD 4.1.2 - (이벤트)
        // EventManager.TriggerEvent("OnBlockPlaced", blockObject);

        return true;
    }

    /// <summary>
    /// GDD 4.1.2 - 지정된 좌표의 블록을 제거합니다.
    /// PlayerBuildController가 호출합니다.
    /// </summary>
    public bool RemoveBlock(Vector3Int position)
    {
        BlockObject blockToRemove = GetBlockAt(position);
        if (blockToRemove == null)
        {
            return false; // 제거할 블록이 없음
        }

        // 1. GDD 4.1.2 - 그리드 딕셔너리에서 제거
        blockGrid.Remove(position);

        // 2. GDD 4.1.2 - 월드에서 게임 오브젝트 파괴
        Destroy(blockToRemove.gameObject);

        // GDD 4.1.2 - (VFX/SFX)
        // VFXManager.Instance.PlayEffect("BlockRemove", GridToWorldPosition(position));

        // (GDD 4.1.2) 이벤트 발행
        // EventManager.TriggerEvent("OnBlockRemoved", position, blockToRemove.data);

        return true;
    }

    /// <summary>
    /// GDD 4.1.2 - 지정된 좌표에 있는 BlockObject를 반환합니다.
    /// </summary>
    public BlockObject GetBlockAt(Vector3Int position)
    {
        if (blockGrid.TryGetValue(position, out BlockObject block))
        {
            return block;
        }
        return null;
    }

    /// <summary>
    /// GDD 4.1.2 - 해당 좌표가 이미 사용 중인지 확인합니다.
    /// </summary>
    public bool IsOccupied(Vector3Int position)
    {
        return blockGrid.ContainsKey(position);
    }

    // --- 2. 저장 & 불러오기 (GDD 4.8) ---

    /// <summary>
    /// GDD 4.8.2 - SaveLoadManager가 호출할 함수.
    /// 현재 그리드에 있는 모든 블록의 상태 데이터를 리스트로 반환합니다.
    /// </summary>
    public List<BlockStateData> GetSaveData()
    {
        // blockGrid의 모든 BlockObject에서 'state'만 추출하여 리스트로 만듭니다.
        return blockGrid.Values.Select(block => block.state).ToList();
    }

    /// <summary>
    /// GDD 4.8.3 - SaveLoadManager가 호출할 함수.
    /// 저장된 블록 상태 리스트를 받아와 월드를 복원합니다.
    /// </summary>
    public void LoadSaveData(List<BlockStateData> data)
    {
        if (data == null) return;

        ClearWorld(); // GDD 4.8.3 - 복원 전 기존 씬 정리

        foreach (BlockStateData state in data)
        {
            // 1. GDD 4.8.3 - ID를 이용해 DataManager에서 원본 SO 데이터를 찾습니다.
            BlockData blockData = DataManager.Instance.GetBlockData(state.dataId);
            if (blockData == null)
            {
                Debug.LogWarning($"[GridSystem] 로드 실패: ID({state.dataId})에 해당하는 BlockData를 찾을 수 없습니다.");
                continue;
            }

            // 2. GDD 4.8.3 - 저장된 정보로 블록을 다시 설치합니다.
            bool success = PlaceBlock(blockData, state.position, state.rotation);

            if (success)
            {
                // 3. GDD 4.8.3 - (중요) PlaceBlock이 만든 기본 state를
                // 파일에서 로드한 'state' (버퍼, 타이머 등 포함)로 덮어씌웁니다.
                GetBlockAt(state.position).state = state;
            }
        }
    }

    /// <summary>
    /// GDD 4.8.3 - 로드하기 전, 현재 월드의 모든 블록을 파괴하고 딕셔너리를 비웁니다.
    /// </summary>
    public void ClearWorld()
    {
        // (중요) ToList()로 복사본을 만들어야 순회 중 딕셔너리 변경 오류가 안 생깁니다.
        foreach (BlockObject block in blockGrid.Values.ToList())
        {
            Destroy(block.gameObject);
        }

        blockGrid.Clear();
    }

    // --- 3. 좌표 변환 헬퍼 ---

    /// <summary>
    /// 월드 좌표(예: Raycast Hit)를 그리드 좌표(Vector3Int)로 변환합니다.
    /// </summary>
    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(relativePos.x / gridCellSize);
        int y = Mathf.FloorToInt(relativePos.y / gridCellSize);
        int z = Mathf.FloorToInt(relativePos.z / gridCellSize);
        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// 그리드 좌표(Vector3Int)를 블록을 생성할 월드 좌표로 변환합니다.
    /// </summary>
    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        float x = (gridPosition.x * gridCellSize) + gridOrigin.x + (gridCellSize * 0.5f); // 중앙 정렬
        float y = (gridPosition.y * gridCellSize) + gridOrigin.y + (gridCellSize * 0.5f); // 중앙 정렬
        float z = (gridPosition.z * gridCellSize) + gridOrigin.z + (gridCellSize * 0.5f); // 중앙 정렬
        return new Vector3(x, y, z);
    }
}