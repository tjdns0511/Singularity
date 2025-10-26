// Description: 게임 상태 관리, 씬 로딩, 매니저 초기화를 담당하는 최상위 매니저 클래스

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 게임 상태 관리 (GameState), 씬 로딩, 다른 매니저 참조 및 초기화를 담당하기 위한 싱글톤 클래스.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 게임 상태를 나타내기 위한 열거형.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameUI
    }

    /// <summary>
    /// 게임의 현재 상태를 가져오기 위한 프로퍼티. (읽기 전용)
    /// </summary>
    public GameState CurrentState { get; private set; }

    // --- 매니저 참조 ---
    [Header("Manager References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private DataManager dataManager;
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private UIManager uiManager;           // NOTE: 현재 ui 미구현
    [SerializeField] private PuzzleManager puzzleManager;
    // TODO: 구현 시 주석 해제 및 Inspector 연결
    // [SerializeField] private SaveLoadManager saveLoadManager;
    // [SerializeField] private AudioManager audioManager;
    // [SerializeField] private VFXManager vfxManager;

    // --- Public 매니저 접근자 ---
    public InputManager InputManager => inputManager;
    public DataManager DataManager => dataManager;
    public GridSystem GridSystem => gridSystem;
    public ChunkManager ChunkManager => chunkManager;
    public UIManager UIManager => uiManager;
    public PuzzleManager PuzzleManager => puzzleManager;
    // public SaveLoadManager SaveLoadManager => saveLoadManager;
    // public AudioManager AudioManager => audioManager;
    // public VFXManager VFXManager => vfxManager;

    /// <summary>
    /// 싱글톤 초기화 및 매니저 참조 유효성 검사를 수행하기 위한 메서드.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        EnsureManagersExist();
    }

    /// <summary>
    /// 게임 시작 시 초기 게임 상태를 설정하기 위한 메서드.
    /// </summary>
    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 게임 플레이 씬 이름 확인
        if (currentScene == "GameScene")
        {
            ChangeState(GameState.Playing);
        }
        else if (currentScene == "MainMenuScene")
        {
            ChangeState(GameState.MainMenu);
        }
        else
        {
            Debug.LogWarning($"GameManager started in unexpected scene: {currentScene}. Defaulting to MainMenu.");
            ChangeState(GameState.MainMenu);
        }
    }

    /// <summary>
    /// Inspector 참조 매니저 할당 여부를 확인하기 위한 메서드 (디버깅용).
    /// </summary>
    private void EnsureManagersExist()
    {
        if (inputManager == null) Debug.LogError("InputManager is not assigned!");
        if (dataManager == null) Debug.LogError("DataManager is not assigned!");
        if (gridSystem == null) Debug.LogError("GridSystem is not assigned!");
        if (chunkManager == null) Debug.LogError("ChunkManager is not assigned!");
        if (puzzleManager == null) Debug.LogError("PuzzleManager is not assigned!");
        if (uiManager == null) Debug.LogWarning("UIManager is not assigned (May be intended).");
        // TODO: 다른 매니저 null 체크 추가
    }

    // --- 상태 관리 ---

    /// <summary>
    /// 게임 상태를 변경하기 위한 메서드.
    /// </summary>
    /// <param name="newState">변경할 새로운 게임 상태</param>
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState);
        CurrentState = newState;
        Debug.Log($"Game State Changed to: {newState}");
        EnterState(newState);
        // TODO: 상태 변경 이벤트 발행
    }

    /// <summary>
    /// 특정 게임 상태 진입 시 로직을 처리하기 위한 메서드.
    /// </summary>
    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                InputManager?.EnableUIInput();
                UIManager?.ShowMainMenu(); // TODO: UIManager 구현 필요
                // TODO: 월드 초기화 로직 필요
                break;
            case GameState.Loading:
                Time.timeScale = 1f;
                InputManager?.DisableAllInput();
                UIManager?.ShowLoadingScreen(); // TODO: UIManager 구현 필요
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                InputManager?.EnableGameplayInput();
                UIManager?.ShowHUD(); // TODO: UIManager 구현 필요
                // TODO: 게임 시스템 재개 로직 필요
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                InputManager?.EnableUIInput();
                UIManager?.ShowPauseMenu(); // TODO: UIManager 구현 필요
                break;
            case GameState.GameUI:
                Time.timeScale = 1f; // 기본값 유지 (필요시 0f로 변경)
                InputManager?.EnableUIInput();
                // NOTE: GameUI 상태 변경은 UI 여는 쪽에서 호출.
                break;
        }
    }

    /// <summary>
    /// 특정 게임 상태 종료 시 정리 로직을 처리하기 위한 메서드.
    /// </summary>
    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                UIManager?.HideMainMenu(); // TODO: UIManager 구현 필요
                break;
            case GameState.Loading:
                UIManager?.HideLoadingScreen(); // TODO: UIManager 구현 필요
                break;
            case GameState.Playing:
                // TODO: Playing 종료 시 자동저장 되게할까
                break;
            case GameState.Paused:
                Time.timeScale = 1f;
                UIManager?.HidePauseMenu(); // TODO: UIManager 구현 필요
                break;
            case GameState.GameUI:
                // NOTE: UI 닫고 Playing 상태 복귀는 UI 스크립트에서 처리.
                break;
        }
    }

    // --- 씬 관리 ---

    /// <summary>
    /// 씬을 동기 방식으로 로드하기 위한 메서드.
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadScene(string sceneName)
    {
        ChangeState(GameState.Loading);
        SceneManager.LoadScene(sceneName);
        // NOTE: 새 씬 로드 후 상태 변경은 새 씬에서 처리 필요.
    }

    /// <summary>
    /// 씬을 비동기 방식으로 로드하기 위한 메서드.
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadSceneAsync(string sceneName)
    {
        ChangeState(GameState.Loading);
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    /// <summary>
    /// 비동기 씬 로딩 코루틴.
    /// </summary>
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UIManager?.UpdateLoadingProgress(progress); // TODO: UIManager 구현 필요

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        // NOTE: 새 씬 활성화 후 상태 변경은 새 씬에서 처리 필요.
    }

    // --- 게임 종료 ---

    /// <summary>
    /// 게임 애플리케이션을 종료하기 위한 메서드.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        // TODO: 종료 전 데이터 저장 등 처리
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}