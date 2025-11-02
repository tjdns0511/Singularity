// In Assets/ScriptableObjects/ChunkRecipeData.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GDD 3.3.1 - '고대 기술 융합' (청크 생성 미니게임)의 레시피 데이터입니다.
/// PuzzleManager가 조합 성공/실패를 판정할 때 사용합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewChunkRecipe", menuName = "Singularity/Data/Chunk Recipe Data")]
public class ChunkRecipeData : ScriptableObject, IDataWithId
{
    [Header("Data ID")]
    [Tooltip("DataManager에서 이 데이터를 찾기 위한 고유 ID")]
    public string id;

    // DataManager의 IDataWithId 인터페이스 구현
    public string ID => id;

    [Header("Combination")]
    [Tooltip("S등급을 받기 위한 필수 원소 조합 (GDD 3.3.1)")]
    public List<ItemData> requiredElements;

    [Header("Results (GDD 3.3.1)")]
    [Tooltip("100% 일치 시 생성될 청크 아이템")]
    public ChunkItemData sGradeChunk;

    [Tooltip("75%~ 일치 시 생성될 청크 아이템")]
    public ChunkItemData aGradeChunk;

    [Tooltip("50%~ 일치 시 생성될 청크 아이템")]
    public ChunkItemData bGradeChunk;

    [Tooltip("25%~ 일치 시 생성될 청크 아이템")]
    public ChunkItemData cGradeChunk;

    [Tooltip("조합 실패 또는 낮은 점수일 때 생성될 부산물 (GDD 3.3.1)")]
    public ItemData failureByproduct;
}