// In Assets/Scripts/World/BlockObject.cs

using UnityEngine;

/// <summary>
/// GDD 4.2.1 - 월드에 실제로 배치되는 모든 블록의 기본 클래스입니다.
/// 이 컴포넌트는 모든 블록 프리팹의 루트에 붙어있어야 합니다.
/// </summary>
public class BlockObject : MonoBehaviour
{
    [Tooltip("이 블록의 정적 데이터 (SO 참조)")]
    public BlockData data;

    [Tooltip("이 블록의 동적 상태 (저장/로드용)")]
    public BlockStateData state;

    /// <summary>
    /// 플레이어가 이 블록을 우클릭(상호작용)했을 때 호출됩니다.
    /// </summary>
    public virtual void OnInteract()
    {
        Debug.Log($"[BlockObject] Interacted with: {data.displayName}");
        // UIManager.Instance.ShowBlockInspector(this); // (유저 요청) 인스펙터 UI 표시
    }

    /// <summary>
    /// (7단계용) 기계의 틱 로직 (MachineObject에서 오버라이드)
    /// </summary>
    public virtual void ProcessTick()
    {
        // 기계가 아니면 아무것도 안 함
    }
}