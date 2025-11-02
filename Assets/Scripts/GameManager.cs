using UnityEngine;
using UnityEngine.SceneManagement;
// using System.Resources; // ResourceManager 제거했으므로 주석 처리 또는 삭제

/// <summary>
/// 게임의 전반적인 상태 관리, 씬 로딩, 매니저 초기화를 담당하는 상위 매니저입니다.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        MainMenu, // 메인 메뉴
        Loading,  // 로딩 중
        Playing,  // 인게임 플레이
        Paused,   // 일시정지
        GameUI    // 게임 중 UI 진입 (예: 인벤토리, 제작 메뉴 등)
    }

    public GameState CurrentState { get; private set; }

    // --- 매니저 참조 ---
    [Header("Manager References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private DataManager dataManager;
    [SerializeField] private GridSystem gridSystem;         // GridSystem 참조 추가
    [SerializeField] private ChunkManager chunkManager;     // ChunkManager 참조 추가
    [SerializeField] private UIManager uiManager;           // UIManager 참조 추가 (미구현 상태)
    [SerializeField] private PuzzleManager puzzleManager;   // PuzzleManager 참조 추가
    // [SerializeField] private SaveLoadManager saveLoadManager; // SaveLoadManager 구현 시 추가
    // [SerializeField] private AudioManager audioManager;     // AudioManager 구현 시 추가
    // [SerializeField] private VFXManager vfxManager;         // VFXManager 구현 시 추가

    // --- 다른 매니저들에 대한 Public 접근자 ---
    public InputManager InputManager => inputManager;
    public DataManager DataManager => dataManager;
    public GridSystem GridSystem => gridSystem;
    public ChunkManager ChunkManager => chunkManager;
    public UIManager UIManager => uiManager;
    public PuzzleManager PuzzleManager => puzzleManager;
    // public SaveLoadManager SaveLoadManager => saveLoadManager;
    // public AudioManager AudioManager => audioManager;
    // public VFXManager VFXManager => vfxManager;

    // --- Unity Lifecycle Methods ---

    protected override void Awake()
    {
        base.Awake(); // Singleton 초기화
        // TODO: 필요한 경우 여기서 매니저들의 초기화 순서를 강제할 수 있습니다.
        // (보통은 각 매니저의 Awake에서 처리하지만, 의존성이 복잡할 경우 여기서 관리)
        EnsureManagersExist(); // 참조된 매니저들이 씬에 있는지 확인 (선택적)
    }

    private void Start()
    {
        // 게임 시작 시 초기 상태 설정
        // 현재 활성화된 씬 이름을 기준으로 초기 상태 결정
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "GameScene") // 실제 게임 플레이 씬 이름으로 변경
        {
            ChangeState(GameState.Playing);
        }
        else if (currentScene == "MainMenuScene") // 메인 메뉴 씬 이름으로 변경
        {
            ChangeState(GameState.MainMenu);
        }
        else
        {
            // 예상치 못한 씬에서 시작될 경우 기본값 설정
            Debug.LogWarning($"GameManager started in unexpected scene: {currentScene}. Defaulting to MainMenu.");
            ChangeState(GameState.MainMenu);
        }
    }

    /// <summary>
    /// 필요한 매니저들이 Inspector에 할당되었는지 확인하는 함수 (디버깅용)
    /// </summary>
    private void EnsureManagersExist()
    {
        if (inputManager == null) Debug.LogError("InputManager is not assigned in GameManager!");
        if (dataManager == null) Debug.LogError("DataManager is not assigned in GameManager!");
        if (gridSystem == null) Debug.LogError("GridSystem is not assigned in GameManager!");
        if (chunkManager == null) Debug.LogError("ChunkManager is not assigned in GameManager!");
        // if (uiManager == null) Debug.LogWarning("UIManager is not assigned in GameManager (May be intended if not implemented yet)."); // UIManager는 아직 미구현일 수 있으므로 Warning
        if (puzzleManager == null) Debug.LogError("PuzzleManager is not assigned in GameManager!");
        // 다른 매니저들도 필요에 따라 확인 추가...
    }


    // --- 상태 관리 ---

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState); // 이전 상태 정리
        CurrentState = newState;
        Debug.Log($"Game State Changed to: {newState}");
        EnterState(newState); // 새 상태 진입
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                InputManager?.EnableUIInput(); // UI 입력 활성화
                UIManager?.ShowMainMenu(); // TODO: UIManager에 메인 메뉴 표시 함수 호출
                // TODO: 게임 월드 관련 시스템 비활성화/정리 (예: GridSystem 클리어)
                break;

            case GameState.Loading:
                Time.timeScale = 1f; // 로딩 중에는 시간이 흘러야 할 수도 있고 아닐 수도 있음 (비동기 로딩 기준)
                InputManager?.DisableAllInput(); // 모든 입력 비활성화
                UIManager?.ShowLoadingScreen(); // TODO: UIManager에 로딩 화면 표시 함수 호출
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                InputManager?.EnableGameplayInput(); // 게임 플레이 입력 활성화
                UIManager?.ShowHUD(); // TODO: UIManager에 HUD 표시 함수 호출
                // TODO: 필요한 게임 시스템 활성화/재개 (예: 기계 업데이트 시작)
                break;

            case GameState.Paused:
                Time.timeScale = 0f; // 시간 정지
                InputManager?.EnableUIInput(); // UI 입력 활성화 (일시정지 메뉴 조작)
                UIManager?.ShowPauseMenu(); // TODO: UIManager에 일시정지 메뉴 표시 함수 호출
                // TODO: 게임 플레이 관련 시스템 일시정지 (애니메이션, 물리 등)
                break;

            case GameState.GameUI:
                // Time.timeScale = 0f; // UI 상태일 때 게임 시간을 멈출지 결정 (선택 사항)
                Time.timeScale = 1f; // 기본값은 시간 흐름 유지
                InputManager?.EnableUIInput(); // UI 입력 활성화
                // UIManager의 특정 함수 호출은 UI를 여는 쪽에서 담당 (예: 인벤토리 버튼 클릭 시 UIManager.OpenInventory() 호출하고, 이 함수 내부에서 ChangeState(GameUI) 호출)
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                UIManager?.HideMainMenu(); // TODO: UIManager 메인 메뉴 숨김 함수 호출
                break;

            case GameState.Loading:
                UIManager?.HideLoadingScreen(); // TODO: UIManager 로딩 화면 숨김 함수 호출
                break;

            case GameState.Playing:
                // TODO: Playing 상태를 벗어날 때 정리할 내용 (예: 자동 저장?)
                break;

            case GameState.Paused:
                Time.timeScale = 1f; // 시간 흐름 복구 (필수!)
                UIManager?.HidePauseMenu(); // TODO: UIManager 일시정지 메뉴 숨김 함수 호출
                InputManager?.EnableGameplayInput(); // 일시정지 해제 시 보통 게임 플레이 입력으로 복귀
                break;

            case GameState.GameUI:
                // Time.timeScale = 1f; // GameUI 상태에서 시간을 멈췄었다면 복구
                // UIManager에서 UI 닫을 때 Playing 상태로 돌아가면서 입력 모드도 같이 변경됨
                break;
        }
    }


    // --- 씬 관리 ---
    // (기존 LoadScene, LoadSceneAsync 코드는 변경 없음)
    public void LoadScene(string sceneName)
    {
        ChangeState(GameState.Loading);
        SceneManager.LoadScene(sceneName);
        // 새 씬 로드 완료 후 상태 변경은 새 씬의 Start 등에서 처리 권장
    }

    public void LoadSceneAsync(string sceneName)
    {
        ChangeState(GameState.Loading);
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UIManager?.UpdateLoadingProgress(progress); // TODO: UIManager에 로딩 진행률 업데이트 함수 호출

            if (progress >= 1.0f) // asyncLoad.progress >= 0.9f 대신 Clamp01 사용한 progress 사용
            {
                // UIManager?.ShowPressAnyKeyPrompt(); // 예시: '아무 키나 누르세요' 표시
                // yield return new WaitUntil(() => Input.anyKeyDown); // 예시: 아무 키 입력 대기
                asyncLoad.allowSceneActivation = true; // 로딩 완료 시 바로 활성화
            }
            yield return null;
        }
        // 씬 활성화 후 상태 변경은 새 씬에서!
    }

    // --- 게임 종료 ---
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        // TODO: 게임 종료 전 저장 로직 등 추가 가능
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}