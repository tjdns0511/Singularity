// In Assets/ScriptableObjects/ResourceData.cs

using UnityEngine;

/// <summary>
    [Header("Resource Info")]
    [Tooltip("Display name shown in UI lists")]
    public string resourceName;

/// GDD 4.4.2 - 모든 자원(아이템, 액체, 기체 등)의 기본이 되는 추상 클래스입니다.
/// DataManager가 ID로 접근할 수 있도록 IDataWithId 인터페이스를 구현합니다.
/// </summary>
public abstract class ResourceData : ScriptableObject, IDataWithId
{
    [Header("Data ID")]
    [Tooltip("DataManager에서 이 데이터를 찾기 위한 고유 ID")]
    public string id;

    // DataManager의 IDataWithId 인터페이스 구현
    public string ID => id;

    [Header("UI Display")]
    [Tooltip("UI에 표시될 이름 (GDD 4.6.4)")]
    public string displayName;

    [Tooltip("UI(인벤토리, 핫바 등)에 표시될 아이콘 (GDD 4.6.4)")]
    public Sprite icon;

    [TextArea]
    public string description; // UI 툴팁용 설명
}