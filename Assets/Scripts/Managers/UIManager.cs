// In Assets/Scripts/UIManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button, Image 사용
using TMPro;          // TextMeshProUGUI 사용

/// <summary>
/// 게임의 모든 UI 요소를 관리하는 중앙 관리자입니다. (GDD 4.6)
/// 싱글톤으로 작동하며 HUD, 빌드 메뉴, 청크 생성 창 등 모든 UI 패널을 제어합니다.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("HUD Panels")]
    [Tooltip("HUD 전체 캔버스 그룹")]
    public GameObject hudPanel;

    [Header("HUD Areas (유저 요청)")]
    [Tooltip("좌측 상단 (마일스톤 버튼)")]
    public GameObject topLeftArea;
    [Tooltip("우측 상단 (설정 버튼)")]
    public GameObject topRightArea;
    [Tooltip("좌측 중앙 (마일스톤 트래커)")]
    public GameObject midLeftArea;
    [Tooltip("우측 중앙 (블록 인스펙터)")]
    public GameObject midRight_InspectorPanel; // GDD 4.6.5
    [Tooltip("우측 하단 (레이어 컨트롤)")]
    public GameObject bottomRightArea;

    [Header("Hotbar (유저 요청)")]
    [Tooltip("핫바 슬롯 9개의 부모 패널")]
    public GameObject hotbarPanel;
    [Tooltip("핫바 슬롯 9개 (HotbarSlotUI.cs 스크립트 포함)")]
    public List<HotbarSlotUI> hotbarSlotsUIList = new List<HotbarSlotUI>(9);

    [Header("Build Menu (1순위)")]
    [Tooltip("빌드 메뉴 패널 (GDD 4.6.4)")]
    public GameObject buildMenuPanel;
    [Tooltip("빌드 메뉴의 블록 버튼 프리팹")]
    public GameObject buildButtonPrefab;
    [Tooltip("버튼이 생성될 Grid Layout Group (Scroll View > Content)")]
    public Transform buildButtonContainer;

    [Header("Chunk Creation Menu (2순위)")]
    [Tooltip("청크 생성 패널 (GDD 4.6.4)")]
    public GameObject chunkCreationPanel;
    [Tooltip("'조합' 버튼")]
    public Button combineButton;
    [Tooltip("조합 결과 텍스트")]
    public TextMeshProUGUI resultText;
    [Tooltip("조합 결과 아이콘")]
    public Image resultIcon;
    // (GDD 3.3.1)
    private List<ItemData> currentCombination = new List<ItemData>();

    [Header("Chunk Inventory Menu (3순위)")]
    [Tooltip("청크 인벤토리 패널 (GDD 4.6.4)")]
    public GameObject chunkInventoryPanel;
    [Tooltip("청크 슬롯이 생성될 부모 (Grid Layout Group)")]
    public Transform chunkInventoryContainer;
    [Tooltip("청크 인벤토리 슬롯 프리팹")]
    public GameObject inventorySlotPrefab;

    // --- Unity 생명주기 ---

    protected override void Awake()
    {
        base.Awake();
        // GDD 4.10.1 - 게임 매니저가 매니저 초기화 순서를 관리
    }

    void Start()
    {
        InitializeHUD();
        SubscribeToEvents(); // GDD 4.7 - 이벤트 구독

        // (선택) 초기 UI 상태 갱신
        UpdateHotbarUI();
        UpdateChunkInventoryUI();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents(); // GDD 4.7 - 이벤트 구독 해제
    }

    // --- 이벤트 구독 관리 (GDD 4.7) ---

    private void SubscribeToEvents()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnHotbarChanged += UpdateHotbarUI;
            PlayerInventory.Instance.OnChunkInventoryChanged += UpdateChunkInventoryUI;
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.OnCombinationAttempted += UpdateCombinationResultUI;
        }

        if (combineButton != null)
        {
            combineButton.onClick.AddListener(OnCombineClicked);
        }
    }

    private void UnsubscribeFromEvents()
    {
        // 인스턴스가 파괴되었을 수 있으므로 null 체크
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnHotbarChanged -= UpdateHotbarUI;
            PlayerInventory.Instance.OnChunkInventoryChanged -= UpdateChunkInventoryUI;
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.OnCombinationAttempted -= UpdateCombinationResultUI;
        }

        if (combineButton != null)
        {
            combineButton.onClick.RemoveListener(OnCombineClicked);
        }
    }


    // --- 1. HUD 관련 함수 ---

    /// <summary>
    /// HUD 패널들의 초기 상태를 설정합니다. (인스펙터 숨기기 등)
    /// </summary>
    void InitializeHUD()
    {
        // GDD 4.6.3 - 인스펙터 패널은 기본적으로 숨김
        if (midRight_InspectorPanel != null)
        {
            midRight_InspectorPanel.SetActive(false);
        }
    }

    /// <summary>
    /// PlayerInventory.OnHotbarChanged 이벤트가 호출할 함수
    /// 핫바 UI 9칸을 인벤토리 데이터에 맞춰 새로고침합니다.
    /// </summary>
    public void UpdateHotbarUI()
    {
        if (PlayerInventory.Instance == null) return;

        List<InventorySlot> slotsData = PlayerInventory.Instance.hotbarSlots;

        for (int i = 0; i < hotbarSlotsUIList.Count; i++)
        {
            if (i < 9 && i < slotsData.Count) // 9칸 핫바
            {
                // i번째 UI 슬롯에게 i번째 인벤토리 데이터를 넘겨 갱신시킵니다.
                hotbarSlotsUIList[i].UpdateSlot(slotsData[i]);
            }
        }
    }

    /// <summary>
    /// 핫바의 현재 선택된 슬롯(selectedIndex)에 하이라이트를 줍니다.
    /// </summary>
    public void UpdateHotbarSelection(int selectedIndex)
    {
        for (int i = 0; i < hotbarSlotsUIList.Count; i++)
        {
            if (i < 9)
            {
                hotbarSlotsUIList[i].SetHighlight(i == selectedIndex);
            }
        }
    }

    /// <summary>
    /// (GDD 4.6.5) PlayerBuildController가 블록을 클릭(Raycast)할 때 호출됩니다.
    /// </summary>
    public void ShowBlockInspector(BlockObject block)
    {
        if (block != null)
        {
            midRight_InspectorPanel.SetActive(true);
            // (GDD 4.6.5) 인스펙터 UI에 block.data와 block.state 정보 표시
            // 예: midRight_InspectorPanel.GetComponentInChildren<TextMeshProUGUI>().text = block.data.displayName;
            Debug.Log($"[UIManager] 인스펙터 표시: {block.data.displayName}");
        }
        else
        {
            midRight_InspectorPanel.SetActive(false);
        }
    }

    // --- 2. 빌드 메뉴 관련 함수 (1순위) ---

    /// <summary>
    /// 빌드 메뉴 패널을 켜고 끕니다. (InputManager에서 호출)
    /// </summary>
    public void ToggleBuildMenu()
    {
        bool isActive = !buildMenuPanel.activeSelf;
        buildMenuPanel.SetActive(isActive);

        if (isActive)
        {
            InitializeBuildMenu(); // 메뉴를 열 때마다 갱신
        }
    }

    /// <summary>
    /// (GDD 4.6.4) 빌드 메뉴를 DataManager의 BlockData로 채웁니다.
    /// </summary>
    void InitializeBuildMenu()
    {
        foreach (Transform child in buildButtonContainer)
        {
            Destroy(child.gameObject);
        }

        List<BlockData> allBlocks = DataManager.Instance.GetAllBlockData();

        foreach (BlockData blockData in allBlocks)
        {
            // (GDD 3.5.2 / 9단계)
            // if (TechManager.Instance.IsTechUnlocked(blockData.requiredTechId) == false)
            //     continue; 

            GameObject buttonObj = Instantiate(buildButtonPrefab, buildButtonContainer);

            // (BlockData에 displayName과 icon 필드가 있다고 가정)
            // buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = blockData.displayName;
            // buttonObj.transform.Find("IconImage").GetComponent<Image>().sprite = blockData.icon; 

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnBuildButtonClicked(blockData));
        }
    }

    /// <summary>
    /// 빌드 메뉴의 버튼이 클릭되었을 때 PlayerBuildController에 알립니다.
    /// </summary>
    void OnBuildButtonClicked(BlockData data)
    {
        PlayerBuildController.Instance.SetBuildMode(data);
        buildMenuPanel.SetActive(false); // 메뉴 닫기
    }

    // --- 3. 청크 생성 UI 관련 함수 (2순위) ---

    /// <summary>
    /// 청크 생성 패널을 켜고 끕니다. (InputManager 등에서 호출)
    /// </summary>
    public void ToggleChunkCreationMenu()
    {
        chunkCreationPanel.SetActive(!chunkCreationPanel.activeSelf);
    }

    /// <summary>
    /// (예시) 아이템 슬롯에 아이템 추가하는 함수 (UI에서 호출)
    /// </summary>
    public void AddItemToCombination(ItemData item)
    {
        currentCombination.Add(item);
        // (UI 슬롯 이미지 업데이트 로직 구현 필요)
    }

    /// <summary>
    /// '조합' 버튼 클릭 시 (Start에서 이벤트로 자동 연결됨)
    /// </summary>
    public void OnCombineClicked()
    {
        if (currentCombination.Count == 0) return;

        resultText.text = "조합 중...";
        resultIcon.sprite = null;

        // PuzzleManager에 조합 시도 요청
        PuzzleManager.Instance.AttemptCombination(currentCombination);

        currentCombination.Clear(); // 조합 시도 후 슬롯 비우기
        // (UI 슬롯 비우는 로직 구현 필요)
    }

    /// <summary>
    /// PuzzleManager.OnCombinationAttempted 이벤트가 호출할 함수
    /// </summary>
    void UpdateCombinationResultUI(CombinationResult result)
    {
        resultText.text = result.message;
        if (result.success && result.resultItem != null)
        {
            // (ItemData에 icon 필드가 있다고 가정)
            // resultIcon.sprite = result.resultItem.icon;
        }
    }

    // --- 4. 청크 인벤토리 관련 함수 (3순위) ---

    /// <summary>
    /// 청크 인벤토리 패널을 켜고 끕니다. (InputManager 등에서 호출)
    /// </summary>
    public void ToggleChunkInventoryMenu()
    {
        bool isActive = !chunkInventoryPanel.activeSelf;
        chunkInventoryPanel.SetActive(isActive);

        if (isActive)
        {
            UpdateChunkInventoryUI(); // 켤 때마다 갱신
        }
    }

    /// <summary>
    /// PlayerInventory.OnChunkInventoryChanged 이벤트가 호출할 함수
    /// </summary>
    void UpdateChunkInventoryUI()
    {
        if (PlayerInventory.Instance == null) return;

        // 1. 컨테이너 비우기
        foreach (Transform child in chunkInventoryContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. 인벤토리 목록으로 새로 채우기
        foreach (ChunkItemData chunk in PlayerInventory.Instance.chunkInventory)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, chunkInventoryContainer);

            // (슬롯 UI 설정: 아이콘, 이름 등. ChunkItemData에 필드 필요)
            // slotObj.GetComponent<Image>().sprite = chunk.icon;
            // slotObj.GetComponentInChildren<TextMeshProUGUI>().text = chunk.displayName;

            // 3. 버튼 클릭 시 PlayerBuildController에 연결
            Button button = slotObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnChunkItemClicked(chunk));
        }
    }

    /// <summary>
    /// 청크 인벤토리 슬롯 클릭 시
    /// </summary>
    void OnChunkItemClicked(ChunkItemData chunk)
    {
        // PlayerBuildController의 빌드 모드를 청크 설치로 변경
        PlayerBuildController.Instance.SetBuildMode(chunk);
        chunkInventoryPanel.SetActive(false); // (선택) 인벤토리 창 닫기
    }
}