// In Assets/Scripts/PlayerBuildController.cs

using System;
using UnityEngine;
using UnityEngine.InputSystem; // Mouse.current.position 사용

/// <summary>
/// GDD 4.11 - 플레이어의 건설/제거/상호작용 입력을 처리합니다.
/// InputManager로부터 이벤트를 받아 GridSystem과 ChunkManager에 명령을 내리고,
/// UIManager와 통신하여 고스트(미리보기) 및 인스펙터를 관리합니다.
/// </summary>
public class PlayerBuildController : Singleton<PlayerBuildController>
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayerMask; // 그리드 바닥 레이어
    [SerializeField] private LayerMask blockLayerMask;  // 블록 레이어 (인스펙터용)
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Ghost Preview (GDD 4.11.3)")]
    [SerializeField] private Material ghostValidMaterial;   // Assets/Materials/Ghost_Valid.mat
    [SerializeField] private Material ghostInvalidMaterial; // Assets/Materials/Ghost_Invalid.mat
    private GameObject currentGhostObject; // 현재 설치 미리보기 오브젝트
    private Renderer ghostRenderer;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 180f;
    private float targetRotationY = 0f;

    // --- 빌드 상태 (GDD 4.11.2) ---
    public enum BuildMode { None, Block, Chunk }
    private BuildMode currentBuildMode = BuildMode.None;
    private BlockData selectedBlockData;
    private ChunkItemData selectedChunkData;
    private Vector3Int currentGridPosition;
    private bool canPlace = false; // 현재 위치에 설치 가능한지 여부

    // --- Unity 생명주기 ---

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        SubscribeToInputEvents();
    }

    /// <summary>
    /// InputManager의 이벤트에 함수들을 연결합니다. (유저가 제공한 InputManager 기반으로 수정)
    /// </summary>
    private void SubscribeToInputEvents()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("[BuildController] InputManager가 씬에 없습니다!");
            return;
        }

        // GDD 4.11.2 - 설치/제거/상호작용/회전
        InputManager.Instance.InteractBuildPressed += HandleInteractBuildPressed; // (수정)
        InputManager.Instance.CancelBuildPressed += HandleCancelRemovePressed;   // (수정)
        InputManager.Instance.RotateBuildPressed += HandleRotatePressed;         // (수정)

        // (핫바 연동은 다음 단계에서 InputManager에 구현 필요)
        // InputManager.Instance.OnHotbarKey += HandleHotbarInput;
    }

    private void OnDestroy()
    {
        // 씬 전환 시 이벤트 구독 해제
        if (InputManager.Instance != null)
        {
            InputManager.Instance.InteractBuildPressed -= HandleInteractBuildPressed; // (수정)
            InputManager.Instance.CancelBuildPressed -= HandleCancelRemovePressed;   // (수정)
            InputManager.Instance.RotateBuildPressed -= HandleRotatePressed;         // (수정)
            // InputManager.Instance.OnHotbarKey -= HandleHotbarInput;
        }
    }

    void Update()
    {
        // GDD 4.11.1 - 매 프레임 Raycast를 쏴서 그리드 좌표와 유효성을 계산
        HandleRaycast();

        // GDD 4.11.3 - 고스트(미리보기) 위치 및 머티리얼 업데이트
        UpdateGhostPreview();
    }

    // --- 1. 입력 처리 함수 (InputManager 이벤트 수신) ---

    /// <summary>
    /// (신규) '설치' (좌클릭) 또는 '상호작용' 입력을 통합 처리합니다.
    /// InputManager의 InteractBuildPressed 이벤트에 연결됩니다.
    /// </summary>
    private void HandleInteractBuildPressed()
    {
        if (currentBuildMode == BuildMode.None)
        {
            // 빌드 모드가 아니면, 상호작용(인스펙터) 시도
            HandleInteractInput();
        }
        else
        {
            // 빌드 모드이면, 설치 시도
            HandleBuildInput();
        }
    }

    /// <summary>
    /// '설치' (좌클릭) 입력 처리 (GDD 4.11.2) - (HandleInteractBuildPressed에서 호출됨)
    /// </summary>
    private void HandleBuildInput()
    {
        if (currentBuildMode == BuildMode.None || !canPlace)
        {
            return; // 빌드 모드가 아니거나 설치 불가능한 곳이면 무시
        }

        Quaternion rotation = Quaternion.Euler(0, targetRotationY, 0);

        // GDD 4.11.2 - 블록 설치
        if (currentBuildMode == BuildMode.Block && selectedBlockData != null)
        {
            bool success = GridSystem.Instance.PlaceBlock(selectedBlockData, currentGridPosition, rotation);
            if (success)
            {
                // (GDD 3.5.2) 설치에 성공하면 인벤토리에서 아이템 소모
                // PlayerInventory.Instance.RemoveItem(selectedBlockData.requiredItem, 1);
            }
        }
        // GDD 4.11.2 - 청크 설치
        else if (currentBuildMode == BuildMode.Chunk && selectedChunkData != null)
        {
            bool success = ChunkManager.Instance.PlaceChunk(selectedChunkData, currentGridPosition);
            if (success)
            {
                // GDD 4.3.1 - 인벤토리에서 사용한 청크 아이템 제거
                PlayerInventory.Instance.RemoveChunkItem(selectedChunkData);

                // 청크는 1회용이므로 설치 후 빌드 모드 해제
                SetBuildMode(null as BlockData);
            }
        }
    }

    /// <summary>
    /// (신규) '제거' (우클릭) 또는 '건설 취소' 입력을 통합 처리합니다.
    /// InputManager의 CancelBuildPressed 이벤트에 연결됩니다.
    /// </summary>
    private void HandleCancelRemovePressed()
    {
        // 빌드 모드일 때는 빌드 취소
        if (currentBuildMode != BuildMode.None)
        {
            SetBuildMode(null as BlockData); // SetBuildMode(null) 호출
            return;
        }

        // 빌드 모드가 아닐 때만 블록 제거 시도
        if (canPlace) // (canPlace는 '유효한 그리드 좌표'를 의미하기도 함)
        {
            // TODO: GDD 4.11.2 - 청크 제거 로직 (ChunkManager.RemoveChunk) 추가 필요

            // GDD 4.11.2 - 그리드 시스템에 블록 제거 요청
            GridSystem.Instance.RemoveBlock(currentGridPosition);
        }
    }

    /// <summary>
    /// '상호작용' (F키 또는 마우스 휠 클릭) 입력 처리 (유저 요청) - (HandleInteractBuildPressed에서 호출됨)
    /// </summary>
    private void HandleInteractInput()
    {
        // GDD 4.6.5 - 인스펙터 UI 표시
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, blockLayerMask))
        {
            BlockObject block = hit.collider.GetComponentInParent<BlockObject>();
            if (block != null)
            {
                // UIManager에 블록 정보를 넘겨 인스펙터 표시 요청
                UIManager.Instance.ShowBlockInspector(block);

                // (선택) 블록의 상호작용 함수도 호출
                // block.OnInteract(); 
            }
        }
        else
        {
            // 블록이 아닌 곳을 클릭하면 인스펙터 숨김
            UIManager.Instance.ShowBlockInspector(null);
        }
    }

    /// <summary>
    /// (수정) '회전' (R키) 입력 처리
    /// InputManager의 RotateBuildPressed 이벤트에 연결됩니다. (파라미터 없음)
    /// </summary>
    private void HandleRotatePressed()
    {
        // R키를 누를 때마다 +90도 (단순화)
        targetRotationY += 90f;
    }

    // --- 2. 빌드 모드 설정 (UIManager / InputManager가 호출) ---

    /// <summary>
    /// (GDD 4.6.4) UIManager(빌드 메뉴)가 호출: '블록' 설치 모드로 설정
    /// </summary>
    public void SetBuildMode(BlockData data)
    {
        if (data == null)
        {
            currentBuildMode = BuildMode.None;
            selectedBlockData = null;
            UpdateGhostPrefab(null); // 고스트 숨김
            return;
        }

        currentBuildMode = BuildMode.Block;
        selectedBlockData = data;
        selectedChunkData = null;

        UpdateGhostPrefab(data.prefab); // GDD 4.11.3 - 블록 고스트 생성
        Debug.Log($"[BuildController] 빌드 모드 변경 (Block): {data.displayName}");
    }

    /// <summary>
    /// (GDD 4.6.4) UIManager(청크 인벤토리)가 호출: '청크' 설치 모드로 설정
    /// </summary>
    public void SetBuildMode(ChunkItemData data)
    {
        if (data == null)
        {
            currentBuildMode = BuildMode.None;
            selectedChunkData = null;
            UpdateGhostPrefab(null); // 고스트 숨김
            return;
        }

        currentBuildMode = BuildMode.Chunk;
        selectedChunkData = data;
        selectedBlockData = null;

        UpdateGhostPrefab(data.chunkPrefab); // GDD 4.11.3 - 청크 고스트 생성
        Debug.Log($"[BuildController] 빌드 모드 변경 (Chunk): {data.displayName}");
    }

    // --- 3. Raycast 및 Ghost 처리 (Update에서 매 프레임 실행) ---

    /// <summary>
    /// GDD 4.11.1 - 마우스 위치로 Raycast를 쏴서 그리드 좌표(currentGridPosition)와
    /// 설치 가능 여부(canPlace)를 매 프레임 계산합니다.
    /// </summary>
    private void HandleRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundLayerMask))
        {
            // 1. GDD 4.11.1 - Raycast 좌표를 그리드 좌표로 변환
            currentGridPosition = GridSystem.Instance.WorldToGridPosition(hit.point);

            // 2. GDD 4.11.3 - 설치 유효성 검사
            if (currentBuildMode == BuildMode.Block)
            {
                // GDD 4.1.2 - 블록은 GridSystem이 점유했는지 확인
                canPlace = !GridSystem.Instance.IsOccupied(currentGridPosition);
            }
            else if (currentBuildMode == BuildMode.Chunk)
            {
                // GDD 4.3 - 청크는 ChunkManager가 유효한지 확인
                canPlace = ChunkManager.Instance.IsPlacementValid(currentGridPosition);
            }
            else
            {
                canPlace = true; // 빌드 모드가 아닐 땐 (제거/상호작용) 항상 유효
            }
        }
        else
        {
            canPlace = false; // 레이캐스트 실패 시
        }
    }

    /// <summary>
    /// GDD 4.11.3 - 고스트(미리보기) 오브젝트의 위치, 회전, 머티리얼을 갱신합니다.
    /// </summary>
    private void UpdateGhostPreview()
    {
        if (currentGhostObject == null) return;

        if (canPlace && currentBuildMode != BuildMode.None)
        {
            currentGhostObject.SetActive(true);

            // 1. 위치 설정
            currentGhostObject.transform.position = GridSystem.Instance.GridToWorldPosition(currentGridPosition);

            // 2. 회전 설정 (Block 모드일 때만 부드럽게 회전)
            if (currentBuildMode == BuildMode.Block)
            {
                Quaternion rotation = Quaternion.Euler(0, targetRotationY, 0);
                currentGhostObject.transform.rotation = Quaternion.Lerp(currentGhostObject.transform.rotation, rotation, Time.deltaTime * 15f);
            }

            // 3. 머티리얼 설정
            if (ghostRenderer != null)
            {
                ghostRenderer.material = ghostValidMaterial;
            }
        }
        else if (!canPlace && currentBuildMode != BuildMode.None)
        {
            // 설치 불가능한 위치 (빨간색 고스트)
            currentGhostObject.SetActive(true);
            currentGhostObject.transform.position = GridSystem.Instance.GridToWorldPosition(currentGridPosition);
            if (ghostRenderer != null)
            {
                ghostRenderer.material = ghostInvalidMaterial;
            }
        }
        else
        {
            // 빌드 모드가 아니면 고스트 숨김
            currentGhostObject.SetActive(false);
        }
    }

    /// <summary>
    /// GDD 4.11.3 - 설치할 프리팹이 바뀌면 기존 고스트를 파괴하고 새로 생성합니다.
    /// </summary>
    private void UpdateGhostPrefab(GameObject prefab)
    {
        if (currentGhostObject != null)
        {
            Destroy(currentGhostObject);
            ghostRenderer = null;
        }

        if (prefab != null)
        {
            currentGhostObject = Instantiate(prefab);

            // (최적화) 고스트에서는 Collider와 하위 로직 스크립트들을 비활성화합니다.
            foreach (var collider in currentGhostObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }
            foreach (var block in currentGhostObject.GetComponentsInChildren<BlockObject>())
            {
                block.enabled = false;
            }

            ghostRenderer = currentGhostObject.GetComponentInChildren<Renderer>();
        }
    }
}