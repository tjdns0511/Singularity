using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 사용

// Rarity enum 정의 (Assets/Enums/Rarity.cs 에 있어야 함 - 예시)
// public enum Rarity { None = 0, Common = 1, Uncommon = 2, Rare = 3, Epic = 4, Legendary = 5 }

/// <summary>
/// 청크 생성 미니게임 로직 관리 (Rarity 기반).
/// 조합 시도와 ChunkRecipeData를 비교하여 Rarity 결과를 판정합니다.
/// </summary>
public class PuzzleManager : Singleton<PuzzleManager>
{
    [SerializeField] private PlayerInventory playerInventory; // 결과 아이템 인벤토리 참조

    // 반환할 결과 Rarity 기본값
    private const Rarity DEFAULT_FAIL_RARITY = Rarity.Common;

    protected override void Awake()
    {
        base.Awake();
        if (playerInventory == null) playerInventory = FindAnyObjectByType<PlayerInventory>();
        if (playerInventory == null) Debug.LogError("PuzzleManager requires a PlayerInventory reference!");
    }

    /// <summary>
    /// 플레이어의 원소 조합 시도를 처리하고 Rarity 기반 결과를 반환합니다.
    /// </summary>
    /// <param name="attemptedCombination">플레이어가 시도한 원소(ItemData) 리스트</param>
    /// <returns>조합 결과 (Rarity 등급 및 생성된 아이템)</returns>
    public (Rarity resultRarity, ItemData resultItem) AttemptCombination(List<ItemData> attemptedCombination)
    {
        if (attemptedCombination == null || attemptedCombination.Count == 0)
        {
            Debug.LogWarning("Attempted combination is empty.");
            return (DEFAULT_FAIL_RARITY, null);
        }

        // --- DataManager에서 모든 ChunkRecipeData 가져오기 ---
        // TODO: DataManager에 GetAllChunkRecipeData() 구현 필요
        List<ChunkRecipeData> allChunkRecipes = DataManager.Instance?.GetAllChunkRecipeData() ?? new List<ChunkRecipeData>();
        if (allChunkRecipes.Count == 0) Debug.LogWarning("No Chunk Recipes loaded in DataManager!");

        ChunkRecipeData bestMatchRecipe = null;
        float highestSimilarity = -1f; // 유사도이므로 0 미만으로 초기화
        bool isPerfectMatch = false;

        // 1. 가장 유사도가 높은 레시피 찾기 (완벽 일치 포함)
        foreach (var recipe in allChunkRecipes)
        {
            // 완벽 일치 먼저 확인 (최적화 및 perfectMatchResultChunk 처리 위함)
            if (IsPerfectMatch(attemptedCombination, recipe.requiredElements))
            {
                bestMatchRecipe = recipe;
                highestSimilarity = 1.0f;
                isPerfectMatch = true;
                Debug.Log($"Perfect match found for chunk recipe: {recipe.name}");
                break; // 완벽 일치 찾으면 더 탐색할 필요 없음
            }

            // 완벽하지 않으면 유사도 계산
            float currentSimilarity = CalculateSimilarity(attemptedCombination, recipe.requiredElements);
            if (currentSimilarity > highestSimilarity)
            {
                highestSimilarity = currentSimilarity;
                bestMatchRecipe = recipe;
            }
        }

        // 2. 결과 판정
        if (bestMatchRecipe != null)
        {
            Debug.Log($"Best match recipe: {bestMatchRecipe.name} with similarity: {highestSimilarity}");

            // 완벽 일치이고 특별 보상이 정의된 경우
            if (isPerfectMatch && bestMatchRecipe.perfectMatchResultChunk != null)
            {
                // RarityOutcome 리스트에서 가장 높은 Rarity 찾기 (표시용)
                Rarity highestRarity = Rarity.Legendary;
                if (bestMatchRecipe.rarityOutcomes != null && bestMatchRecipe.rarityOutcomes.Count > 0)
                {
                    // 리스트가 similarity 내림차순 정렬되어 있다고 가정
                    highestRarity = bestMatchRecipe.rarityOutcomes[0].rarity;
                }
                AwardItem(bestMatchRecipe.perfectMatchResultChunk);
                return (highestRarity, bestMatchRecipe.perfectMatchResultChunk); // 완벽 일치 특별 보상 지급
            }

            // 유사도 기반 Rarity 결정 (rarityOutcomes 리스트는 similarity 내림차순 정렬 가정)
            if (bestMatchRecipe.rarityOutcomes != null)
            {
                foreach (var outcome in bestMatchRecipe.rarityOutcomes)
                {
                    if (highestSimilarity >= outcome.minSimilarity) // 최소 유사도 기준 충족 시
                    {
                        if (outcome.resultChunk != null)
                        {
                            AwardItem(outcome.resultChunk);
                            return (outcome.rarity, outcome.resultChunk); // 해당 Rarity 청크 지급
                        }
                        else
                        {
                            // 해당 Rarity 결과 청크가 없으면 부산물 시도
                            Debug.LogWarning($"Outcome defined for Rarity {outcome.rarity} (Similarity >= {outcome.minSimilarity}) but no resultChunk assigned. Checking byproduct.");
                            break; // 더 낮은 Rarity는 볼 필요 없음
                        }
                    }
                }
            }

            // 어떤 Rarity 기준도 만족하지 못했거나, 만족한 Rarity에 결과 청크가 없었을 경우 -> 부산물 지급 시도
            if (bestMatchRecipe.byproductItem != null)
            {
                AwardItem(bestMatchRecipe.byproductItem);
                Debug.Log($"Awarding byproduct: {bestMatchRecipe.byproductItem.name}");
                // 부산물의 Rarity는 어떻게 할지 결정 필요 (예: Common 또는 None)
                return (Rarity.Common, bestMatchRecipe.byproductItem); // 임시로 Common Rarity 부여
            }
        }

        // 3. 일치하는 레시피 없음 또는 부산물도 없음 -> 완전 실패
        Debug.Log("No matching recipe found or no outcome/byproduct defined. Combination failed.");
        return (DEFAULT_FAIL_RARITY, null);
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가하는 헬퍼 함수
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
            // playerInventory.AddItem(itemToAward, quantity); // 일반 아이템 추가 함수 필요
            Debug.LogWarning($"PlayerInventory needs AddItem method for item '{itemToAward.name}'.");
        }
    }


    /// <summary>
    /// 두 아이템 리스트가 완벽히 일치하는지 확인합니다. (순서 고려 또는 무시 옵션)
    /// </summary>
    private bool IsPerfectMatch(List<ItemData> attempt, List<ItemData> recipeElements)
    {
        // 필요에 따라 순서 고려/무시 로직 선택 (이전 답변 코드 참조)
        // 예시: 순서 고려
        if (attempt.Count != recipeElements.Count) return false;
        for (int i = 0; i < attempt.Count; i++)
        {
            if (attempt[i] != recipeElements[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// 두 아이템 리스트 간의 유사도를 계산합니다. (0.0 ~ 1.0)
    /// </summary>
    private float CalculateSimilarity(List<ItemData> attempt, List<ItemData> recipeElements)
    {
        // 필요에 따라 유사도 계산 방식 선택 (이전 답변 코드 참조)
        // 예시: Jaccard Index (고유 원소 종류 기준)
        if (recipeElements == null || recipeElements.Count == 0) return (attempt == null || attempt.Count == 0) ? 1.0f : 0f;
        if (attempt == null || attempt.Count == 0) return 0f;

        HashSet<ItemData> attemptSet = new HashSet<ItemData>(attempt);
        HashSet<ItemData> recipeSet = new HashSet<ItemData>(recipeElements);
        int intersection = attemptSet.Intersect(recipeSet).Count();
        int union = attemptSet.Union(recipeSet).Count();
        if (union == 0) return 1f;
        return (float)intersection / union;
    }
}