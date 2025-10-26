// Assets/ScriptableObjects/BuildingItemData.cs (새 파일)
using UnityEngine;

/// <summary>
/// 건설 메뉴 UI에 표시되고, 특정 BlockData 설치를 나타내는 아이템 데이터입니다.
/// ItemData를 상속받습니다.
/// </summary>
[CreateAssetMenu(fileName = "New Building Item", menuName = "Singularity/Data/Item/Building Item")]
public class BuildingItemData : ItemData // ItemData 상속
{
    [Header("Building Item Specific Data")]
    [Tooltip("이 아이템을 선택했을 때 실제로 월드에 설치될 건물(Block)의 데이터입니다.")]
    public BlockData blockToPlace; // 설치할 BlockData 참조 추가

    // ItemData로부터 상속받는 필드들:
    // public string itemName; // 건물 아이템 이름 (예: "기본 채굴기 아이템")
    // public string description; // 설명
    // public Sprite icon; // 빌드 메뉴에 표시될 아이콘
    // public int maxStackSize = 99; // 필요하다면 스택 가능하게 설정
}