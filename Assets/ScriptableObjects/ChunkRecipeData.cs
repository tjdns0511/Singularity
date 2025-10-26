using UnityEngine;
using System.Collections.Generic;

// Rarity enum 정의 (Assets/Enums/Rarity.cs 에 있어야 함 - 예시)
// public enum Rarity { None = 0, Common = 1, Uncommon = 2, Rare = 3, Epic = 4, Legendary = 5 }

/// <summary>
/// 청크 생성 미니게임 전용 레시피 데이터입니다. (Rarity 기반)
/// 원소 조합 및 결과 Rarity를 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Chunk Recipe", menuName = "Singularity/Data/Chunk Recipe (Rarity)")]
public class ChunkRecipeData : ScriptableObject
{
    [System.Serializable] // Inspector 노출 및 직렬화
    public class RarityOutcome
    {
        public Rarity rarity = Rarity.Common; // 기본값 설정

        [Range(0f, 1f)]
        public float minSimilarity = 0.5f; // 예시 기본값
        public ChunkItemData resultChunk;
    }

    [Header("Chunk Recipe Information")]
    public string recipeId;

    public List<ItemData> requiredElements;

    [Header("Combination Outcomes by Rarity")]
    public List<RarityOutcome> rarityOutcomes;

    // 완벽 일치(Similarity 1.0) 시 특별 보상 (선택 사항)
    public ChunkItemData perfectMatchResultChunk;

    [Header("Byproduct (Optional)")]
    public ItemData byproductItem;

    // 기획 문서 3.3: 부산물 분석 힌트 (선택 사항)
    // public List<string> hints;

    /// <summary>
    /// Editor에서 데이터 검증 시 사용 (선택 사항)
    /// RarityOutcomes 리스트가 minSimilarity 내림차순으로 정렬되었는지 확인
    /// </summary>
    private void OnValidate()
    {
        if (rarityOutcomes != null && rarityOutcomes.Count > 1)
        {
            // Sort the list by minSimilarity descending to ensure correct logic in PuzzleManager
            rarityOutcomes.Sort((a, b) => b.minSimilarity.CompareTo(a.minSimilarity));
        }
    }
}