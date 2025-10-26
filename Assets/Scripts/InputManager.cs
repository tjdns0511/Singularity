using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 상호작용 및 시스템 입력(설치, 제거, 회전, 일시정지 등)을 관리하는 싱글톤 매니저입니다.
/// (카메라 이동/회전 제외 버전)
/// </summary>
public class InputManager : Singleton<InputManager>
{
    [SerializeField] private InputActionAsset playerControlsAsset;
    private PlayerControls playerControls;

    // --- 버튼 입력 이벤트들 ---
    public event Action InteractBuildPressed;  // 상호작용 / 설치 (마우스 좌클릭)
    public event Action CancelBuildPressed;    // 제거 / 건설 취소 (마우스 우클릭)
    public event Action RotateBuildPressed;    // 회전 (R)
    public event Action FlipBuildPressed;      // 반전 (F)
    public event Action PausePressed;          // 일시정지 (ESC)

    public event Action MenuEscapePressed;
    public event Action MenuMouseLaftPressed;

    // --- Unity Lifecycle Methods ---

    protected override void Awake()
    {
        base.Awake();
        InitializeInputActions();
    }

    private void OnEnable()
    {
        // GameManager 상태 변경 시 적절한 입력 활성화 필요
        // 예: GameManager.Instance.OnGameStateChanged += HandleGameStateChange;
    }

    private void OnDisable()
    {
        DisableAllInput();
        // 예: GameManager.Instance.OnGameStateChanged -= HandleGameStateChange;
    }

    // --- 초기화 ---

    private void InitializeInputActions()
    {
        if (playerControlsAsset == null)
        {
            Debug.LogError("PlayerControls InputActionAsset이 InputManager에 할당되지 않았습니다!");
            return;
        }

        playerControls = new PlayerControls(); // PlayerControls는 생성한 C# 클래스 이름

        // Gameplay 액션 맵의 버튼 액션들에 콜백 함수 연결
        playerControls.Gameplay.InteractNBuild.performed += ctx => InteractBuildPressed?.Invoke();
        playerControls.Gameplay.CancelNDeleteBuild.performed += ctx => CancelBuildPressed?.Invoke();
        playerControls.Gameplay.RotateBuild.performed += ctx => RotateBuildPressed?.Invoke();
        playerControls.Gameplay.RotateBuild.performed += ctx => FlipBuildPressed?.Invoke();
        playerControls.Gameplay.PauseNMenu.performed += ctx => PausePressed?.Invoke();

        // TODO: UI 액션 맵
        playerControls.UI.escape.performed += ctx => MenuEscapePressed?.Invoke();
        playerControls.UI.LeftClick.performed += ctx => MenuMouseLaftPressed?.Invoke();
    }


    //입력 활성화/비활성화

    /// <summary>
    /// 게임 플레이 입력 활성화
    /// </summary>
    public void EnableGameplayInput()
    {
        DisableAllInput();
        playerControls.Gameplay.Enable();
        Debug.Log("Gameplay Input Enabled (Actions Only)");
    }

    /// <summary>
    /// UI 조작 관련 입력 활성화
    /// </summary>
    public void EnableUIInput()
    {
        DisableAllInput();
        playerControls.UI.Enable();
        Debug.Log("UI Input Enabled");
    }

    /// <summary>
    /// 모든 입력 액션 맵을 비활성화
    /// </summary>
    public void DisableAllInput()
    {
        playerControls.Gameplay.Disable();
        playerControls.UI.Disable();
        Debug.Log("All Input Disabled");
    }

    // GameManager 상태 변경에 따라 입력을 관리하는 함수 (예시)
    /*
    private void HandleGameStateChange(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Playing)
        {
            EnableGameplayInput();
        }
        else if (newState == GameManager.GameState.Paused || newState == GameManager.GameState.MainMenu)
        {
            EnableUIInput();
        }
        else // Loading 등
        {
            DisableAllInput();
        }
    }
    */
}