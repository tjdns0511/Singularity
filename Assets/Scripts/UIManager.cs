using UnityEngine;
using UnityEngine.UI; // 기본 UI 요소 사용 시
using TMPro; // TextMeshPro 사용 시
using System.Collections.Generic; // List 사용

/// <summary>
/// 게임의 주요 UI 패널 및 요소들을 관리하는 싱글톤 매니저.
/// GameManager와 연동하여 상태에 따른 UI 표시 및 입력을 관리합니다.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("UI Panel References")]
    [SerializeField] private GameObject hudPanel;            // HUD 패널 참조 추가
    [SerializeField] private GameObject mainMenuPanel;       // 메인 메뉴 패널 참조 추가
    [SerializeField] private GameObject loadingScreenPanel;  // 로딩 화면 패널 참조 추가
    [SerializeField] private GameObject pauseMenuPanel;      // 일시정지 메뉴 패널 참조 추가
    [SerializeField] private GameObject buildMenuPanel;
    [SerializeField] private GameObject chunkInventoryPanel;
    [SerializeField] private GameObject chunkCreationPanel;
    // [SerializeField] private GameObject techTreePanel; // 기술 트리 UI (기획 문서 4.6)
    // [SerializeField] private GameObject tooltipPanel; // 아이템 정보 툴팁 (기획 문서 4.6)

    [Header("Loading Screen Elements")]
    [SerializeField] private Slider loadingProgressBar; // 로딩 진행률 표시 슬라이더 (예시)
    // [SerializeField] private TextMeshProUGUI loadingProgressText; // 로딩 진행률 텍스트 (예시)

    [Header("Build Menu Elements")]
    [SerializeField] private Transform buildItemListContainer;
    [SerializeField] private GameObject buildItemButtonPrefab;

    [Header("Chunk Inventory Elements")]
    [SerializeField] private Transform chunkItemListContainer;
    [SerializeField] private GameObject chunkItemButtonPrefab;

    [Header("Chunk Creation Elements")]
    // TODO: 청크 생성 UI 요소 참조 추가
    [SerializeField] private Transform elementSelectionContainer; // 원소 선택 영역
    [SerializeField] private Button combineButton; // 조합 시도 버튼
    [SerializeField] private TextMeshProUGUI combinationResultText; // 조합 결과 표시 텍스트 (예시)
    private List<ItemData> selectedElements = new List<ItemData>(); // 현재 선택된 원소 리스트

    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager; // GameManager 참조 추가
    [SerializeField] private InputManager inputManager; // InputManager 참조 추가
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerBuildController playerBuildController;
    [SerializeField] private PuzzleManager puzzleManager;

    protected override void Awake()
    {
        base.Awake();
        // 의존성 자동 찾기 (null일 경우)
        if (gameManager == null) gameManager = GameManager.Instance;
        if (inputManager == null) inputManager = InputManager.Instance;
        if (playerInventory == null) playerInventory = FindAnyObjectByType<PlayerInventory>();
        if (playerBuildController == null) playerBuildController = FindAnyObjectByType<PlayerBuildController>();
        if (puzzleManager == null) puzzleManager = PuzzleManager.Instance;

        // 초기 UI 상태 설정 (모든 패널 숨김)
        HideAllPanels(); // 함수 이름 변경 (Close -> Hide)

        // 인벤토리 변경 시 UI 업데이트 이벤트 구독
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += UpdateChunkInventoryUI;
        }

        // 청크 생성 조합 버튼 리스너 추가 (Start에서 해도 무방)
        combineButton?.onClick.AddListener(AttemptChunkCombination);
    }

    private void Start()
    {
        // 게임 시작 시 UI 초기화
        PopulateBuildMenu();
        UpdateChunkInventoryUI();
        // TODO: PopulateElementSelection(); // 청크 생성 UI 원소 목록 채우기
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateChunkInventoryUI;
        }
        combineButton?.onClick.RemoveListener(AttemptChunkCombination);
    }

    // --- 패널 관리 및 상태 연동 ---

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel?.SetActive(true);
    }

    public void HideMainMenu()
    {
        mainMenuPanel?.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        HideAllPanels();
        loadingScreenPanel?.SetActive(true);
        if (loadingProgressBar != null) loadingProgressBar.value = 0; // 로딩 바 초기화
        // if (loadingProgressText != null) loadingProgressText.text = "Loading... 0%";
    }

    public void HideLoadingScreen()
    {
        loadingScreenPanel?.SetActive(false);
    }

    public void UpdateLoadingProgress(float progress)
    {
        if (loadingProgressBar != null) loadingProgressBar.value = progress;
        // if (loadingProgressText != null) loadingProgressText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }

    public void ShowHUD()
    {
        HideAllPanels();
        hudPanel?.SetActive(true);
        // 기획 문서 4.6: HUD 내용 표시 (현재 선택 아이템, 복원 진행률 등) 업데이트 로직 필요
    }

    public void HideHUD() // 필요시 HUD 숨기는 함수
    {
        hudPanel?.SetActive(false);
    }


    public void ShowPauseMenu()
    {
        // HUD 위에 표시될 수 있으므로 HideAllPanels() 호출 안 함
        pauseMenuPanel?.SetActive(true);
        // gameManager?.ChangeState(GameManager.GameState.Paused); // Pause 상태 변경은 외부(예: InputManager)에서 하는 것이 더 적절할 수 있음
    }

    public void HidePauseMenu()
    {
        pauseMenuPanel?.SetActive(false);
        // 게임 재개 로직 (예: GameManager.ChangeState(Playing))은 메뉴의 'Resume' 버튼 등에서 처리
    }


    public void OpenBuildMenu()
    {
        HideAllPanels();
        buildMenuPanel?.SetActive(true);
        gameManager?.ChangeState(GameManager.GameState.GameUI); // 게임 UI 상태로 변경
        // InputManager?.EnableUIInput(); // ChangeState 내부에서 처리되도록 GameManager 수정 권장
    }

    public void OpenChunkInventory()
    {
        HideAllPanels();
        chunkInventoryPanel?.SetActive(true);
        gameManager?.ChangeState(GameManager.GameState.GameUI);
        // InputManager?.EnableUIInput();
    }

    public void OpenChunkCreation()
    {
        HideAllPanels();
        chunkCreationPanel?.SetActive(true);
        gameManager?.ChangeState(GameManager.GameState.GameUI);
        // InputManager?.EnableUIInput();
        // TODO: ResetElementSelection(); // 원소 선택 초기화
    }

    // 모든 게임 내 메뉴/패널 닫고 게임 플레이 상태로 복귀
    public void CloseAllPanels() // HideAllPanels과 역할 분리
    {
        HideAllPanels(); // 모든 패널 숨김

        // 현재 게임 상태가 UI 상태일 경우에만 Playing 상태로 변경
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.GameUI)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
            // InputManager?.EnableGameplayInput(); // ChangeState 내부에서 처리되도록 GameManager 수정 권장
        }
        // 빌드 모드 해제 (UI 닫을 때 항상 해제할지 결정 필요)
        playerBuildController?.SetBuildMode(false);
    }

    // 단순히 모든 패널을 비활성화하는 내부 함수
    private void HideAllPanels()
    {
        hudPanel?.SetActive(false); // HUD도 일단 숨김 (ShowHUD에서 다시 켬)
        mainMenuPanel?.SetActive(false);
        loadingScreenPanel?.SetActive(false);
        pauseMenuPanel?.SetActive(false);
        buildMenuPanel?.SetActive(false);
        chunkInventoryPanel?.SetActive(false);
        chunkCreationPanel?.SetActive(false);
        // techTreePanel?.SetActive(false);
        // tooltipPanel?.SetActive(false);
    }


    // --- UI 업데이트 ---

    /// <summary>
    /// 빌드 메뉴에 건설 가능한 건물 아이템 목록을 채웁니다. (수정됨)
    /// </summary>
    private void PopulateBuildMenu()
    {
        if (buildItemListContainer == null || buildItemButtonPrefab == null || DataManager.Instance == null) return;

        foreach (Transform child in buildItemListContainer) Destroy(child.gameObject);

        // --- DataManager에서 BlockData 대신 BuildingItemData 가져오기 ---
        List<BuildingItemData> buildableItems = DataManager.Instance.GetAllBuildingItemData();
        // TODO: 기술 해금 여부에 따라 필터링하는 로직 추가

        foreach (BuildingItemData itemData in buildableItems) // BuildingItemData 사용
        {
            if (itemData.blockToPlace == null) // 연결된 BlockData가 없는 아이템은 건너뜀 (오류 방지)
            {
                Debug.LogWarning($"BuildingItemData '{itemData.name}' has no BlockData assigned in 'blockToPlace'. Skipping.");
                continue;
            }

            GameObject buttonGO = Instantiate(buildItemButtonPrefab, buildItemListContainer);
            // --- 버튼 내용 설정 (ItemButtonUI 사용) ---
            var itemButtonUI = buttonGO.GetComponent<ItemButtonUI>();
            if (itemButtonUI != null)
            {
                itemButtonUI.Setup(itemData); // BuildingItemData로 설정 (ItemData 받도록 오버로드 사용)
            }
            else // 임시 텍스트 설정
            {
                TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = itemData.resourceName;
            }

            // --- 버튼 클릭 이벤트 수정 ---
            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                // 클릭 시 BuildingItemData를 PlayerBuildController로 전달
                button.onClick.AddListener(() => {
                    playerBuildController?.SetBuildMode(true, buildingItem: itemData); // buildingItem 인자 사용
                    CloseAllPanels();
                });
            }
        }
    }

    private void UpdateChunkInventoryUI()
    {
        if (chunkItemListContainer == null || chunkItemButtonPrefab == null || playerInventory == null) return;

        foreach (Transform child in chunkItemListContainer) Destroy(child.gameObject);

        List<InventorySlot> chunkItems = playerInventory.GetChunkInventory();

        foreach (InventorySlot slot in chunkItems)
        {
            if (slot.itemDataRef is ChunkItemData chunkData)
            {
                GameObject buttonGO = Instantiate(chunkItemButtonPrefab, chunkItemListContainer);
                // --- 버튼 내용 설정 ---
                var itemButtonUI = buttonGO.GetComponent<ItemButtonUI>(); // 버튼 프리팹에 아이템 정보 표시용 스크립트가 있다고 가정
                if (itemButtonUI != null)
                {
                    itemButtonUI.Setup(chunkData, slot.quantity); // 아이콘, 이름, 수량 등 설정
                }
                else // 임시: 텍스트만 설정
                {
                    TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null) buttonText.text = $"{chunkData.resourceName} x{slot.quantity}"; // ItemData에 itemName 필드 필요
                }


                // --- 버튼 클릭 이벤트 ---
                Button button = buttonGO.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => {
                        playerBuildController?.SetBuildMode(true, chunkData: chunkData);
                        CloseAllPanels(); // 메뉴 닫고 게임 상태로 복귀
                    });
                }
            }
        }
    }

    // --- 청크 생성 UI 관련 ---

    // TODO: 사용 가능한 원소 목록을 UI에 표시하는 함수
    // private void PopulateElementSelection() { ... }

    // TODO: 플레이어가 원소 버튼 클릭 시 selectedElements 리스트에 추가/제거하는 함수
    // public void SelectElement(ItemData element) { ... }
    // public void DeselectElement(ItemData element) { ... }

    // TODO: 선택된 원소 목록 초기화 함수
    // private void ResetElementSelection() { selectedElements.Clear(); UpdateSelectedElementsUI(); }

    /// <summary>
    /// 조합 버튼 클릭 시 PuzzleManager에 조합 시도 요청
    /// </summary>
    private void AttemptChunkCombination()
    {
        if (puzzleManager == null || selectedElements == null || selectedElements.Count == 0)
        {
            if (combinationResultText != null) combinationResultText.text = "원소를 선택하세요!";
            return;
        }

        // PuzzleManager 호출
        (Rarity resultRarity, ItemData resultItem) = puzzleManager.AttemptCombination(selectedElements);

        // 결과 처리 (UI 피드백)
        if (combinationResultText != null)
        {
            if (resultItem != null)
            {
                combinationResultText.text = $"조합 성공! [{resultRarity}] {resultItem.resourceName} 획득!";
                // TODO: 성공 시각/사운드 효과 (VFXManager, AudioManager 연동)
            }
            else
            {
                combinationResultText.text = "조합 실패...";
                // TODO: 실패 시각/사운드 효과
            }
        }

        // 조합 후 선택된 원소 초기화
        // ResetElementSelection();
        selectedElements.Clear(); // 임시 초기화

        // TODO: 조합에 사용된 원소 인벤토리에서 제거 로직 필요
    }

    // TODO: 기획 문서 4.6: 기술 트리 UI 관련 함수
    // public void OpenTechTree() { ... }
    // public void UpdateTechTreeUI() { ... }

    // TODO: 기획 문서 4.6: 아이템 정보 툴팁 표시/숨김 함수
    // public void ShowTooltip(ItemData item, Vector2 position) { ... }
    // public void HideTooltip() { ... }

    // TODO: 기획 문서 4.6: 피드백 제공 함수 (예: 자원 부족 알림)
    // public void ShowNotification(string message) { ... }
}

// --- (선택 사항) 버튼 프리팹에 붙일 스크립트 예시 ---
/*
public class ItemButtonUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemQuantityText;

    public void Setup(ItemData itemData, int quantity = -1) // 수량 -1은 표시 안 함
    {
        if (itemIcon != null) itemIcon.sprite = itemData.icon; // ItemData에 icon 필드 필요
        if (itemNameText != null) itemNameText.text = itemData.itemName; // ItemData에 itemName 필드 필요
        if (itemQuantityText != null)
        {
            itemQuantityText.gameObject.SetActive(quantity > 0); // 수량이 있을 때만 표시
            itemQuantityText.text = $"x{quantity}";
        }
    }
     public void Setup(BlockData blockData) // BlockData 오버로드
    {
        if (itemIcon != null) itemIcon.sprite = blockData.icon; // BlockData에 icon 필드 필요
        if (itemNameText != null) itemNameText.text = blockData.blockName; // BlockData에 blockName 필드 필요
        if (itemQuantityText != null) itemQuantityText.gameObject.SetActive(false); // 블록은 수량 표시 안 함
    }
}
*/