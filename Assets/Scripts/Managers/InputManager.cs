// Description: 플레이어 입력 처리 및 관련 이벤트 발생을 위한 싱글톤 매니저. (New Input System 기반)

using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 플레이어 입력(상호작용, 빌드, UI 등) 처리 및 이벤트 발생을 위한 싱글톤 클래스.
/// </summary>
public class InputManager : Singleton<InputManager>
{
    [SerializeField] private InputActionAsset playerControlsAsset;
    private PlayerControls playerControls;

    // --- Public 입력 이벤트 ---
    public event Action InteractBuildPressed;
    public event Action CancelBuildPressed;
    public event Action RotateBuildPressed;
    public event Action FlipBuildPressed; // TODO: Input Actions 에셋에 'FlipBuild' 액션 추가 및 할당 필요
    public event Action PausePressed;
    public event Action MenuEscapePressed;
    public event Action MenuMouseLeftPressed; // 이름 오타 수정 (Laft -> Left)

    /// <summary>
    /// 싱글톤 및 Input Action 초기화를 위한 메서드.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        InitializeInputActions();
        if (playerControls == null) Debug.LogError("PlayerControls initialization failed!");
    }

    /// <summary>
    /// 컴포넌트 비활성화 시 모든 입력을 비활성화하기 위한 메서드.
    /// </summary>
    private void OnDisable()
    {
        DisableAllInput();
    }

    /// <summary>
    /// Input Action Asset 기반 인스턴스 생성 및 이벤트 콜백 연결을 위한 메서드.
    /// </summary>
    private void InitializeInputActions()
    {
        if (playerControlsAsset == null)
        {
            Debug.LogError("PlayerControls InputActionAsset이 InputManager에 할당되지 않았습니다!");
            return;
        }

        playerControls = new PlayerControls();

        if (playerControls == null)
        {
            Debug.LogError("Failed to create PlayerControls instance!");
            return;
        }

        // Gameplay 액션 맵 콜백 연결
        playerControls.Gameplay.InteractNBuild.performed += ctx => InteractBuildPressed?.Invoke();
        playerControls.Gameplay.CancelNDeleteBuild.performed += ctx => CancelBuildPressed?.Invoke();
        playerControls.Gameplay.RotateBuild.performed += ctx => RotateBuildPressed?.Invoke();
        // playerControls.Gameplay.FlipBuild.performed += ctx => FlipBuildPressed?.Invoke(); // FlipBuild 액션 연결
        playerControls.Gameplay.PauseNMenu.performed += ctx => PausePressed?.Invoke();

        // UI 액션 맵 콜백 연결
        playerControls.UI.escape.performed += ctx => MenuEscapePressed?.Invoke();
        playerControls.UI.LeftClick.performed += ctx => MenuMouseLeftPressed?.Invoke();
    }

    /// <summary>
    /// 게임 플레이 Input Action Map 활성화를 위한 메서드.
    /// </summary>
    public void EnableGameplayInput()
    {
        DisableAllInput();
        if (playerControls != null)
        {
            playerControls.Gameplay.Enable();
        }
        else Debug.LogError("Cannot enable Gameplay input: PlayerControls is null!");
    }

    /// <summary>
    /// UI Input Action Map 활성화를 위한 메서드.
    /// </summary>
    public void EnableUIInput()
    {
        DisableAllInput();
        if (playerControls != null)
        {
            playerControls.UI.Enable();
        }
        else Debug.LogWarning("Cannot enable UI input: PlayerControls is null!");
    }

    /// <summary>
    /// 모든 Input Action Map 비활성화를 위한 메서드.
    /// </summary>
    public void DisableAllInput()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Disable();
        playerControls.UI.Disable();
    }
}