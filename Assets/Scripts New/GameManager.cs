using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 상위의 메인 매니저
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        MainMenu, // 메인 메뉴
        Loading,  // 로딩 중
        Playing,  // 인게임 플레이
        Paused,    // 일시정지
        GameUI //게임중 UI 진입
    }

    public GameState CurrentState { get; private set; }

    //매니저 참조
    [Header("Manager References")]
    [SerializeField] private InputManager inputManager;
    //[SerializeField] private UIManager uiManager;  //아직 미구현
    //[SerializeField] private WorldManager worldManager;  //아직 미구현
    [SerializeField] private ResourceManager resourceManager;
    //[SerializeField] private FactoryManager factoryManager;  //아직 미구현

    // 다른 매니저들에 대한 public 접근자 (필요한 경우)
    public InputManager InputManager => inputManager;
    //public UIManager UIManager => uiManager;
    //public WorldManager WorldManager => worldManager;
    public ResourceManager ResourceManager => resourceManager;
    //public FactoryManager FactoryManager => factoryManager;

    // --- Unity Lifecycle Methods ---

    protected override void Awake()
    {
        base.Awake(); // Singleton 초기화 (중복 방지 등)
        // 게임 시작 시 초기 상태 설정 (예: 메인 메뉴)
        // CurrentState = GameState.MainMenu; // 또는 게임 시작 씬에 따라 다르게 설정
    }

    private void Start()
    {
        // TODO: 게임 시작 시 필요한 초기화 로직 (예: 첫 씬 로드 후 상태 변경)
        // 예시: 게임이 바로 시작하는 씬이라면
        if (SceneManager.GetActiveScene().name == "GameScene") // 실제 게임 씬 이름으로 변경하세요.
        {
            ChangeState(GameState.Playing);
        }
        else
        {
            ChangeState(GameState.MainMenu);
        }
    }

    // --- 상태 관리 ---

    /// <summary>
    /// 게임 상태 변경 로직
    /// </summary>
    /// <param name="newState">변경할 새로운 게임 상태</param>
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return; // 이미 같은 상태면 변경하지 않음

        // 이전 상태에 대한 정리 로직 (필요한 경우)
        ExitState(CurrentState);

        CurrentState = newState;
        Debug.Log($"Game State Changed to: {newState}"); // 상태 변경 로그 출력

        // 새 상태에 대한 초기화 로직
        EnterState(newState);
    }

    /// <summary>
    /// 특정 상태 진입 시 실행될 로직
    /// </summary>
    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                // TODO: 메인 메뉴 상태 초기화
                Time.timeScale = 1f;
                InputManager?.EnableUIInput();
                break;
            case GameState.Loading:
                // TODO: 로딩 상태 초기화
                Time.timeScale = 0f;
                InputManager?.DisableAllInput();
                break;
            case GameState.Playing:
                // TODO: 인게임 플레이 상태 초기화
                Time.timeScale = 1f;
                InputManager?.EnableGameplayInput();
                break;
            case GameState.Paused:
                // TODO: 일시정지 상태 초기화
                Time.timeScale = 0f;
                InputManager?.EnableUIInput();
                break;
            case GameState.GameUI:
                // TODO: 게임중 UI 진입 상태 초기화
                Time.timeScale = 1f;
                InputManager?.EnableUIInput();
                break;
        }
    }

    /// <summary>
    /// 특정 상태 벗어날시 실행 로직
    /// </summary>
    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                // TODO: 메인 메뉴 상태 정리 로직 (예: 메인 메뉴 UI 비활성화)
                break;
            case GameState.Loading:
                // TODO: 로딩 상태 정리 로직 (예: 로딩 UI 숨김)
                break;
            case GameState.Playing:
                // TODO: 인게임 플레이 상태 정리 로직 (예: 플레이어 입력 비활성화?)
                break;
            case GameState.Paused:
                // TODO: 일시정지 상태 정리 로직 (예: 일시정지 메뉴 UI 비활성화)
                Time.timeScale = 1f; // 일시정지 해제 시 시간 흐름 복구
                break;
                // 다른 상태들에 대한 case 추가
        }
    }


    // --- 씬 관리 ---

    /// <summary>
    /// 지정된 이름의 씬을 로드합니다. (동기 방식)
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    public void LoadScene(string sceneName)
    {
        ChangeState(GameState.Loading); // 로딩 상태로 변경
        // 로딩 씬이 있다면 로딩 씬을 먼저 로드 후, 목표 씬을 비동기로 로드하는 것이 일반적입니다.
        // 여기서는 간단하게 바로 목표 씬을 로드합니다.
        SceneManager.LoadScene(sceneName);
        // 목표 씬 로드 완료 후, 해당 씬의 Start나 Awake 등에서 GameState를 Playing 등으로 변경해야 합니다.
        // 또는 SceneManager.sceneLoaded 이벤트에 콜백을 등록하여 상태를 변경할 수 있습니다.
    }

    /// <summary>
    /// 지정된 이름의 씬을 비동기 방식으로 로드합니다. (로딩 화면 구현 시 유용)
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    public void LoadSceneAsync(string sceneName)
    {
        ChangeState(GameState.Loading);
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        // 로딩 씬이 있다면 여기서 로딩 씬 로드
        // SceneManager.LoadScene("LoadingScene");
        // yield return null; // 로딩 씬 로드 대기

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 씬 로딩 완료 후 바로 활성화하지 않음

        // 로딩 진행률 표시 등 (uiManager 활용)
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // 0.9에서 로딩이 멈추므로 보정
            // uiManager.UpdateLoadingProgress(progress); // 예시: 로딩 UI 업데이트

            if (asyncLoad.progress >= 0.9f)
            {
                // 로딩 완료 후 '아무 키나 누르세요' 등 처리가 필요하면 여기서 대기
                // if (Input.anyKeyDown) // 예시
                // {
                //      asyncLoad.allowSceneActivation = true;
                // }

                // 바로 활성화하려면
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // 씬 로드 및 활성화 완료 후 상태 변경 (주의: 이 코루틴은 새 씬에서 파괴될 수 있으므로,
        // 새 씬의 특정 스크립트에서 상태 변경을 처리하는 것이 더 안정적일 수 있습니다.)
        // ChangeState(GameState.Playing); // 또는 해당 씬의 초기 상태로 변경
    }

    // --- 게임 종료 ---
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중일 경우 종료
#endif
    }
}