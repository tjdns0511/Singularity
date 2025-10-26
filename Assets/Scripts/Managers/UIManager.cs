// Description: 게임 UI 요소 관리 및 표시 업데이트를 위한 싱글톤 매니저. (현재 미구현)

using UnityEngine;

/// <summary>
/// 게임 내 UI 요소(패널, 버튼 등) 관리 및 상태에 따른 표시 업데이트를 위한 싱글톤 클래스.
/// </summary>
public class UIManager : MonoBehaviour // 싱글톤으로 만들거나 전용 UI 관리 오브젝트에 부착
{
    // --- UI Panel References ---
    // TODO: Inspector에서 실제 UI 패널 게임 오브젝트 연결 필요
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    // TODO: 인벤토리, 빌드 메뉴, 퍼즐 UI 등 다른 패널 참조 추가

    // --- Loading Screen Elements ---
    // TODO: 로딩 진행률 표시용 UI 요소 참조 추가 (예: Slider, Text)
    // [SerializeField] private UnityEngine.UI.Slider loadingProgressBar;
    // [SerializeField] private TMPro.TextMeshProUGUI loadingProgressText;

    /// <summary>
    /// 메인 메뉴 UI 표시를 위한 메서드.
    /// </summary>
    public void ShowMainMenu()
    {
        HideAllPanels(); // 다른 패널 숨김 (선택 사항)
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        // else Debug.LogWarning("MainMenuPanel is not assigned!"); // 로그 최소화
    }

    /// <summary>
    /// 메인 메뉴 UI 숨김을 위한 메서드.
    /// </summary>
    public void HideMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    /// <summary>
    /// 로딩 화면 표시를 위한 메서드.
    /// </summary>
    public void ShowLoadingScreen()
    {
        HideAllPanels();
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);
        UpdateLoadingProgress(0); // 초기 진행률 0으로 설정
    }

    /// <summary>
    /// 로딩 화면 숨김을 위한 메서드.
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
    }

    /// <summary>
    /// 로딩 진행률 UI 업데이트를 위한 메서드.
    /// </summary>
    /// <param name="progress">진행률 (0.0 ~ 1.0)</param>
    public void UpdateLoadingProgress(float progress)
    {
        // TODO: 로딩 진행률 표시 UI 업데이트 로직 구현
        // if (loadingProgressBar != null) loadingProgressBar.value = progress;
        // if (loadingProgressText != null) loadingProgressText.text = $"Loading... {progress * 100:F0}%";
    }

    /// <summary>
    /// 인게임 HUD(Heads-Up Display) 표시를 위한 메서드.
    /// </summary>
    public void ShowHUD()
    {
        HideAllPanels();
        if (hudPanel != null) hudPanel.SetActive(true);
    }

    /// <summary>
    /// 인게임 HUD 숨김을 위한 메서드.
    /// </summary>
    public void HideHUD()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    /// <summary>
    /// 일시정지 메뉴 표시를 위한 메서드.
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    /// <summary>
    /// 일시정지 메뉴 숨김을 위한 메서드.
    /// </summary>
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    // TODO: 인벤토리, 빌드 메뉴, 퍼즐 UI 등 다른 UI 표시/숨김 메서드 추가

    /// <summary>
    /// 모든 주요 UI 패널을 숨기기 위한 내부 헬퍼 메서드.
    /// </summary>
    private void HideAllPanels()
    {
        // null 체크 후 비활성화
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        // TODO: 다른 패널들도 숨김 처리
    }
}