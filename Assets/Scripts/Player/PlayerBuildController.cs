using UnityEngine;
using UnityEngine.InputSystem; // New Input System 사용

/// <summary>
/// 플레이어의 건설 관련 입력(설치, 제거, 회전 등)을 처리하고
/// GridSystem 및 ChunkManager와 상호작용하여 블록/청크를 배치/제거합니다.
/// </summary>
public class PlayerBuildController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private ChunkManager chunkManager; // 청크 설치/제거 및 유효성 검사 위해 필요
    [SerializeField] private Camera mainCamera;

    [Header("Build Settings")]
    [SerializeField] private BuildingItemData currentBuildingItem; // 현재 선택된 건물 아이템
    [SerializeField] private ChunkItemData currentChunkToBuild; // 현재 선택된 설치 청크 데이터
    [SerializeField] private int minBuildLayer = 0;
    [SerializeField] private int maxBuildLayer = 19;
    [SerializeField] private LayerMask groundLayerMask; // Raycast가 충돌할 바닥 레이어

    private BlockData blockDataToPlace; // 실제로 설치할 BlockData

    [Header("Ghost/Preview")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;
    private GameObject ghostInstance;
    private Renderer ghostRenderer;
    private bool isBuildMode = true; // true: 블록/청크 건설 모드, false: 일반 모드 (임시)

    private Plane gridPlane = new Plane(Vector3.up, Vector3.zero); // 건설 기준면
    private int currentBuildLayer = 0;
    private Vector3Int currentGridPosition; // 현재 마우스 커서가 가리키는 그리드 좌표
    private bool currentPlacementValid = false; // 현재 위치에 건설 가능한지 여부
    private float currentRotationY = 0f; // 현재 건설 회전값

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (inputManager == null) inputManager = InputManager.Instance; // 싱글톤으로 찾기
        if (gridSystem == null) gridSystem = GridSystem.Instance; // 싱글톤으로 찾기
        if (chunkManager == null) chunkManager = ChunkManager.Instance; // 싱글톤으로 찾기

        UpdateBuildPlane();
    }

    private void OnEnable()
    {
        // InputManager 이벤트 구독
        if (inputManager != null)
        {
            inputManager.InteractBuildPressed += HandleBuildInput;
            inputManager.CancelBuildPressed += HandleRemoveInput;
            inputManager.RotateBuildPressed += HandleRotateInput; // 회전 입력 이벤트 구독
            // TODO: F키 반전 입력 구독 (FlipBuildPressed)
        }
        else
        {
            Debug.LogError("InputManager not found!");
        }
    }

    private void OnDisable()
    {
        // InputManager 이벤트 구독 해제
        if (inputManager != null)
        {
            inputManager.InteractBuildPressed -= HandleBuildInput;
            inputManager.CancelBuildPressed -= HandleRemoveInput;
            inputManager.RotateBuildPressed -= HandleRotateInput;
        }
        // 고스트 객체 제거
        DestroyGhost();
    }

    private void Update()
    {
        HandleLayerInput(); // E, Q 키로 빌드 레이어 조절 (구버전 방식 유지)
        UpdatePlacementInfo(); // 마우스 위치 추적 및 유효성 검사
        UpdateGhost(); // 고스트/미리보기 업데이트
    }

    /// <summary>
    /// 빌드 모드를 활성화/비활성화합니다. (수정됨: BuildingItemData 또는 ChunkItemData 받도록)
    /// </summary>
    public void SetBuildMode(bool buildMode, BuildingItemData buildingItem = null, ChunkItemData chunkData = null)
    {
        isBuildMode = buildMode;
        currentBuildingItem = buildingItem; // BuildingItemData 저장
        currentChunkToBuild = chunkData;    // ChunkItemData 저장
        currentRotationY = 0f;

        // 설치할 BlockData 설정
        blockDataToPlace = null; // 일단 초기화
        if (currentBuildingItem != null && currentBuildingItem.blockToPlace != null)
        {
            blockDataToPlace = currentBuildingItem.blockToPlace; // BuildingItem에서 BlockData 추출
        }
        else if (currentChunkToBuild != null)
        {
            // 청크 설치 모드일 때는 blockDataToPlace가 null이어야 함
        }

        // 고스트 초기화 (설치할 블록 또는 청크가 있을 때)
        if (isBuildMode && (blockDataToPlace != null || currentChunkToBuild != null))
        {
            InitializeGhost();
        }
        else
        {
            DestroyGhost();
        }
        Debug.Log($"Build Mode: {isBuildMode}, BuildingItem: {currentBuildingItem?.name}, ChunkItem: {currentChunkToBuild?.name}, BlockToPlace: {blockDataToPlace?.name}");
    }


    /// <summary>
    /// 마우스 좌클릭 시 호출 (건설 시도) (수정됨)
    /// </summary>
    private void HandleBuildInput()
    {
        if (!isBuildMode || !currentPlacementValid) return;

        if (blockDataToPlace != null) // 설치할 BlockData가 있으면 블록 건설
        {
            gridSystem.PlaceBlock(blockDataToPlace, currentGridPosition, Quaternion.Euler(0, currentRotationY, 0));
            // TODO: 건물 아이템 인벤토리에서 제거 로직 추가 (필요시)
        }
        else if (currentChunkToBuild != null) // 설치할 청크 아이템이 있으면 청크 설치
        {
            chunkManager.PlaceChunk(currentChunkToBuild, currentGridPosition);
            // TODO: 청크 아이템 인벤토리에서 제거 로직 추가 (PlayerInventory 연동)
        }
    }

    /// <summary>
    /// 마우스 우클릭 시 호출 (제거 시도)
    /// </summary>
    private void HandleRemoveInput()
    {
        if (!IsValidPosition(currentGridPosition)) return; // 마우스가 유효한 위치가 아니면 무시

        // 빌드 모드일 때는 건설 취소 또는 현재 선택 해제 등의 기능으로 활용 가능 (선택사항)
        if (isBuildMode)
        {
            Debug.Log("Build cancelled / deselected.");
            SetBuildMode(false); // 예시: 빌드 모드 해제
            return;
        }

        // 일반 모드에서 제거 시도
        // 먼저 해당 위치에 블록이 있는지 확인
        if (gridSystem.GetBlockAt(currentGridPosition) != null)
        {
            gridSystem.RemoveBlock(currentGridPosition);
            // TODO: 블록 제거 시 자원 반환 로직 추가
        }
        // else if (chunkManager.IsChunkAt(currentGridPosition)) // ChunkManager에 해당 좌표 청크 확인 메서드 필요
        // {
        //     // TODO: 청크 제거 로직 구현 (ChunkManager.RemoveChunk)
        // }
    }

    /// <summary>
    /// R 키 입력 시 호출 (회전) (수정됨)
    /// </summary>
    private void HandleRotateInput()
    {
        // 건물 아이템이 선택되었고, 실제로 설치할 블록이 있을 때만 회전 가능
        if (!isBuildMode || currentBuildingItem == null || blockDataToPlace == null) return;

        currentRotationY = (currentRotationY + 90f) % 360f;
    }


    /// <summary>
    /// 마우스 위치로부터 그리드 좌표와 건설 유효성을 계산합니다.
    /// </summary>
    private void UpdatePlacementInfo()
    {
        Vector3 worldPosition = GetMouseWorldPositionOnPlane();
        if (!IsValidPosition(worldPosition))
        {
            currentGridPosition = new Vector3Int(-999, -999, -999);
            currentPlacementValid = false;
            return;
        }

        currentGridPosition = Vector3Int.FloorToInt(worldPosition);
        currentGridPosition.y = currentBuildLayer; // Y 좌표는 현재 빌드 레이어로 고정

        // 건설 유효성 검사 (수정됨)
        if (blockDataToPlace != null) // 설치할 BlockData 기준으로 검사
        {
            currentPlacementValid = !gridSystem.IsOccupied(currentGridPosition) && chunkManager.IsPositionInActiveChunk(currentGridPosition);
        }
        else if (currentChunkToBuild != null) // 청크 설치 유효성
        {
            currentPlacementValid = chunkManager.IsPlacementValid(currentGridPosition);
        }
        else
        {
            currentPlacementValid = false;
        }
    }

    /// <summary>
    /// 마우스 위치에 해당하는 월드 좌표를 계산합니다. (Raycast 방식 - 바닥 충돌)
    /// </summary>
    private Vector3 GetMouseWorldPositionWithRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, groundLayerMask))
        {
            return hit.point;
        }
        return new Vector3(-999, -999, -999); // 유효하지 않음 표시
    }

    /// <summary>
    /// 마우스 위치에 해당하는 월드 좌표를 계산합니다. (Plane Raycast 방식 - 구버전 방식)
    /// </summary>
    private Vector3 GetMouseWorldPositionOnPlane()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            // Y 값은 currentBuildLayer로 고정되므로 X, Z만 사용
            return new Vector3(worldPosition.x, currentBuildLayer, worldPosition.z);
        }
        return new Vector3(-999, -999, -999); // 유효하지 않음 표시
    }

    /// <summary>
    /// 주어진 좌표가 유효한지 (Raycast 성공했는지) 확인합니다.
    /// </summary>
    private bool IsValidPosition(Vector3 position)
    {
        // Vector3Int 버전도 추가
        return position.x != -999;
    }
    private bool IsValidPosition(Vector3Int position)
    {
        return position.x != -999;
    }


    /// <summary>
    /// 고스트/미리보기 오브젝트를 초기화합니다. (수정됨)
    /// </summary>
    private void InitializeGhost()
    {
        DestroyGhost();

        GameObject prefabToUse = null;
        // 설치할 BlockData의 프리팹 사용
        if (blockDataToPlace != null && blockDataToPlace.prefab != null)
        {
            prefabToUse = blockDataToPlace.prefab;
        }
        // 또는 설치할 청크 아이템의 프리팹 사용
        else if (currentChunkToBuild != null && currentChunkToBuild.chunkPrefab != null)
        {
            prefabToUse = currentChunkToBuild.chunkPrefab;
        }

        if (prefabToUse != null)
        {
            ghostInstance = Instantiate(prefabToUse);
            // ... (고스트 설정 나머지 부분은 동일) ...
            ghostRenderer = ghostInstance.GetComponentInChildren<Renderer>();
            if (ghostRenderer == null) {/*...*/}
            Collider col = ghostInstance.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    /// <summary>
    /// 고스트/미리보기 오브젝트의 위치, 회전, 머티리얼을 업데이트합니다.
    /// </summary>
    private void UpdateGhost()
    {
        if (ghostInstance == null) return;

        if (IsValidPosition(currentGridPosition) && isBuildMode)
        {
            ghostInstance.SetActive(true);
            ghostInstance.transform.position = currentGridPosition; // 그리드 좌표에 정확히 위치
            ghostInstance.transform.rotation = Quaternion.Euler(0, currentRotationY, 0); // 현재 회전값 적용

            // 유효성에 따라 머티리얼 변경
            if (ghostRenderer != null)
            {
                ghostRenderer.material = currentPlacementValid ? ghostValidMaterial : ghostInvalidMaterial;
            }
        }
        else
        {
            ghostInstance.SetActive(false); // 유효하지 않거나 빌드 모드가 아니면 숨김
        }
    }

    /// <summary>
    /// 고스트/미리보기 오브젝트를 파괴합니다.
    /// </summary>
    private void DestroyGhost()
    {
        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
            ghostRenderer = null;
        }
    }


    // --- 빌드 레이어 조절 (구버전 방식) ---
    private void HandleLayerInput()
    {
        bool layerChanged = false;
        // 임시로 GetKeyDown 사용, InputManager 이벤트 방식으로 바꾸는 것이 좋음
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentBuildLayer++;
            layerChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentBuildLayer--;
            layerChanged = true;
        }

        if (layerChanged)
        {
            currentBuildLayer = Mathf.Clamp(currentBuildLayer, minBuildLayer, maxBuildLayer);
            UpdateBuildPlane();
            Debug.Log($"Current Build Layer: {currentBuildLayer}");
            UpdatePlacementInfo(); // 레이어 변경 시 유효성 재검사
        }
    }

    private void UpdateBuildPlane()
    {
        // 건설 기준면의 높이를 현재 빌드 레이어에 맞춤
        gridPlane.SetNormalAndPosition(Vector3.up, new Vector3(0, currentBuildLayer, 0));
    }
}