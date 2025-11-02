// In Assets/ScriptableObjects/BlockData.cs

using UnityEngine;

/// <summary>
/// GDD 4.4.2 - 월드 그리드에 설치될 수 있는 모든 블록(기계, 벽, 벨트 등)의
/// 기본이 되는 추상 클래스입니다.
/// </summary>
public abstract class BlockData : ScriptableObject, IDataWithId
{
    [Header("Data ID")]
    [Tooltip("DataManager에서 이 데이터를 찾기 위한 고유 ID")]
    public string id;

    // DataManager의 IDataWithId 인터페이스 구현
    public string ID => id;

    [Header("UI Display")]
    [Tooltip("UI(빌드 메뉴 등)에 표시될 이름 (GDD 4.6.4)")]
    public string displayName;

    [Tooltip("UI(빌드 메뉴 등)에 표시될 아이콘 (GDD 4.6.4)")]
    public Sprite icon;

    [TextArea]
    public string description;

    [Header("Placement")]
    [Tooltip("월드에 설치(PlaceBlock)될 블록의 원본 프리팹")]
    public GameObject prefab; // GDD 4.2 (BlockObject 컴포넌트 포함)

    // (GDD 3.5.2 / 9단계)
    // [Header("Progression")]
    // [Tooltip("이 블록을 해금하기 위해 필요한 기술 ID")]
    // public string requiredTechId; 
}