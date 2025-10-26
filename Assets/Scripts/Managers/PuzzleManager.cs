// Description: 청크 생성 미니게임 로직(조합 판정) 관리를 위한 싱글톤 매니저.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 청크 생성 미니게임 로직(Rarity 기반 조합 판정) 관리를 위한 싱글톤 클래스.
/// </summary>
public class PuzzleManager : Singleton<PuzzleManager>
{
    // 조합 결과 아이템을 받을 PlayerInventory 참조.
    [SerializeField] private PlayerInventory playerInventory;

    // 조합 완전 실패 시 반환할 기본 Rarity 값.
    private const Rarity DEFAULT_FAIL_RARITY = Rarity.Common;

    /// <summary>
    /// 싱글톤 초기화 및 PlayerInventory 참조 확인을 위한 메서드.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        // PlayerInventory 참조 자동 찾기 (Inspector에서 할당되지 않은 경우)
        if (playerInventory == null) playerInventory = FindAnyObjectByType<PlayerInventory>();
        // 최종 확인
        if (playerInventory == null) Debug.LogError("PuzzleManager requires a PlayerInventory reference!");
    }

    /// <summary>
    /// 플레이어의 원소 조합 시도 처리 및 결과(Rarity, 아이템) 반환을 위한 메서드.
    /// </summary>
    /// <param name="attemptedCombination">플레이어가 시도한 원소(ItemData) 리스트</param>
    /// <returns>조합 결과 Rarity 및 생성된 ItemData (실패 시 null)</returns>
    public (Rarity resultRarity, ItemData resultItem) AttemptCombination(List<ItemData> attemptedCombination)
    {
        if (attemptedCombination == null || attemptedCombination.Count == 0)
        {
            return (DEFAULT_FAIL_RARITY, null);
        }

        // DataManager에서 모든 청크 레시피 로드
        List<ChunkRecipeData> allChunkRecipes = DataManager.Instance?.GetAllChunkRecipeData() ?? new List<ChunkRecipeData>();

        ChunkRecipeData bestMatchRecipe = null;
        float highestSimilarity = -1f; // 유사도 (0.0 ~ 1.0)
        bool isPerfectMatch = false;

        // 1. 가장 유사도가 높은 레시피 찾기 (완벽 일치 우선)
        foreach (var recipe in allChunkRecipes)
        {
            if (IsPerfectMatch(attemptedCombination, recipe.requiredElements)) // 완벽 일치 확인
            {
                bestMatchRecipe = recipe;
                highestSimilarity = 1.0f;
                isPerfectMatch = true;
                break; // 완벽 일치 시 탐색 종료
            }

            // 완벽하지 않으면 유사도 계산 및 최고 유사도 레시피 갱신
            float currentSimilarity = CalculateSimilarity(attemptedCombination, recipe.requiredElements);
            if (currentSimilarity > highestSimilarity)
            {
                highestSimilarity = currentSimilarity;
                bestMatchRecipe = recipe;
            }
        }

        // 2. 결과 판정 및 아이템 지급
        if (bestMatchRecipe != null)
        {
            // Debug.Log($"Best match recipe: {bestMatchRecipe.name} (Similarity: {highestSimilarity})"); // 로그 최소화

            // 완벽 일치 + 특별 보상 청크가 있는 경우
            if (isPerfectMatch && bestMatchRecipe.perfectMatchResultChunk != null)
            {
                AwardItem(bestMatchRecipe.perfectMatchResultChunk);
                // 완벽 일치 시 최고 Rarity 반환 (레시피 정의 기준)
                Rarity topRarity = (bestMatchRecipe.rarityOutcomes != null && bestMatchRecipe.rarityOutcomes.Count > 0)
                                   ? bestMatchRecipe.rarityOutcomes[0].rarity // 첫 번째(최고 유사도) Rarity 사용
                                   : Rarity.Legendary; // 기본값 (rarityOutcomes 없거나 비어있을 경우)
                return (topRarity, bestMatchRecipe.perfectMatchResultChunk);
            }

            // 유사도 기반 Rarity 결과 판정 (rarityOutcomes 리스트는 similarity 내림차순 정렬 가정)
            if (bestMatchRecipe.rarityOutcomes != null)
            {
                foreach (var outcome in bestMatchRecipe.rarityOutcomes)
                {
                    if (highestSimilarity >= outcome.minSimilarity) // 최소 유사도 충족 시
                    {
                        if (outcome.resultChunk != null) // 해당 Rarity 결과 청크가 정의되어 있으면
                        {
                            AwardItem(outcome.resultChunk);
                            return (outcome.rarity, outcome.resultChunk); // 해당 청크 지급
                        }
                        else
                        {
                            // 결과 청크 없으면 더 낮은 Rarity 볼 필요 없이 부산물 처리로 넘어감
                            break;
                        }
                    }
                }
            }

            // Rarity 기준 미달 또는 결과 청크 부재 시 -> 부산물 지급 시도
            if (bestMatchRecipe.byproductItem != null)
            {
                AwardItem(bestMatchRecipe.byproductItem);
                return (Rarity.Common, bestMatchRecipe.byproductItem); // 부산물은 Common Rarity 부여 (임시)
            }
        }

        // 3. 일치 레시피 없음 또는 지급할 아이템 없음 -> 완전 실패
        return (DEFAULT_FAIL_RARITY, null);
    }

    /// <summary>
    /// PlayerInventory에 아이템 추가를 위한 내부 헬퍼 메서드.
    /// </summary>
    private void AwardItem(ItemData itemToAward, int quantity = 1)
    {
        if (itemToAward == null || playerInventory == null) return;

        if (itemToAward is ChunkItemData chunkItem)
        {
            playerInventory.AddChunkItem(chunkItem, quantity);
        }
        else
        {
            // playerInventory.AddItem(itemToAward, quantity); // TODO: 일반 아이템 추가 함수 호출 필요
            Debug.LogWarning($"PlayerInventory needs AddItem method for item '{itemToAward.name}'.");
        }
    }

    /// <summary>
    /// 두 아이템 리스트 완벽 일치 여부 확인을 위한 내부 메서드. (순서 고려)
    /// </summary>
    /// <returns>일치 여부</returns>
    private bool IsPerfectMatch(List<ItemData> attempt, List<ItemData> recipeElements)
    {
        // TODO: 조합 순서 무시 로직 필요 시 수정 (예: Count 확인 후 Set 비교)
        if (attempt == null || recipeElements == null || attempt.Count != recipeElements.Count)
        {
            return false;
        }
        for (int i = 0; i < attempt.Count; i++)
        {
            if (attempt[i] != recipeElements[i]) return false; // ScriptableObject는 직접 비교 가능
        }
        return true;
    }

    /// <summary>
    /// 두 아이템 리스트 간 유사도 계산을 위한 내부 메서드 (0.0 ~ 1.0).
    /// 현재 Jaccard Index (고유 원소 종류 기준) 사용.
    /// </summary>
    /// <returns>계산된 유사도 값</returns>
    private float CalculateSimilarity(List<ItemData> attempt, List<ItemData> recipeElements)
    {
        // TODO: 필요 시 다른 유사도 계산 로직으로 변경 (예: 순서, 개수 가중치 등)
        if (recipeElements == null || recipeElements.Count == 0) return (attempt == null || attempt.Count == 0) ? 1.0f : 0f;
        if (attempt == null || attempt.Count == 0) return 0f;

        // 고유 원소 Set 생성
        HashSet<ItemData> attemptSet = new HashSet<ItemData>(attempt);
        HashSet<ItemData> recipeSet = new HashSet<ItemData>(recipeElements);

        // 교집합(Intersection)과 합집합(Union) 크기 계산
        int intersection = attemptSet.Intersect(recipeSet).Count();
        int union = attemptSet.Union(recipeSet).Count();

        // Jaccard Index 계산: (교집합 크기) / (합집합 크기) 두 집합 사이의 유사도 계산
        if (union == 0) return 1f; // 둘 다 빈 경우 (이론상 발생 안 함)
        return (float)intersection / union;
    }
}