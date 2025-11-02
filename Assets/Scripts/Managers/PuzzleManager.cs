// In Assets/Scripts/PuzzleManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GDD 3.3 '고대 기술 융합' (청크 생성 미니게임)의 핵심 로직을 관리합니다.
/// UIManager로부터 조합 시도를 받아 레시피와 대조하고, 결과를 PlayerInventory에 추가합니다.
/// </summary>
public struct CombinationResult
{
    public bool success;      // 조합 성공 여부 (C등급 이상)
    public string message;    // UI에 표시될 메시지
    public ItemData resultItem; // 생성된 ChunkItemData 또는 ByproductData
    public string grade;        // S, A, B, C, F 등급
}

public class PuzzleManager : Singleton<PuzzleManager>
{
    /// <summary>
    /// UIManager가 조합 결과를 UI에 반영할 수 있도록 알리는 이벤트입니다.
    /// </summary>
    public static event Action<CombinationResult> OnCombinationAttempted;

    /// <summary>
    /// UIManager의 '조합' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void AttemptCombination(List<ItemData> elements)
    {
        if (elements == null || elements.Count == 0)
        {
            OnCombinationAttempted?.Invoke(new CombinationResult
            {
                success = false,
                message = "원소를 넣어주세요.",
                resultItem = null,
                grade = "F"
            });
            return;
        }

        // 1. DataManager에서 모든 레시피 정보를 가져옵니다.
        List<ChunkRecipeData> allRecipes = DataManager.Instance.GetAllChunkRecipes(); // (DataManager에 이 함수 구현 필요)

        CombinationResult bestResult;
        bestResult.success = false;
        bestResult.message = "조합 실패... 부산물이 생성되었습니다.";
        bestResult.resultItem = DataManager.Instance.GetDefaultByproduct(); // (실패 시 기본 부산물 - DataManager에 구현 필요)
        bestResult.grade = "F";

        float bestScore = 0f;

        // 2. 모든 레시피와 대조하여 가장 점수가 높은 결과를 찾습니다.
        foreach (ChunkRecipeData recipe in allRecipes)
        {
            float score = CalculateCombinationScore(elements, recipe.requiredElements);

            if (score > bestScore)
            {
                bestScore = score;

                // GDD 3.3.1 - 점수에 따라 등급 결정
                if (score >= 1.0f) // 100% 일치
                {
                    bestResult.success = true;
                    bestResult.message = $"완벽한 성공! (S)";
                    bestResult.resultItem = recipe.sGradeChunk; // S등급 청크
                    bestResult.grade = "S";
                }
                else if (score >= 0.75f) // 75% 이상
                {
                    bestResult.success = true;
                    bestResult.message = "성공! (A)";
                    bestResult.resultItem = recipe.aGradeChunk; // A등급 청크
                    bestResult.grade = "A";
                }
                else if (score >= 0.5f) // 50% 이상
                {
                    bestResult.success = true;
                    bestResult.message = "그럭저럭... (B)";
                    bestResult.resultItem = recipe.bGradeChunk; // B등급 청크
                    bestResult.grade = "B";
                }
                else if (score >= 0.25f) // 25% 이상
                {
                    bestResult.success = true;
                    bestResult.message = "부족한 조합 (C)";
                    bestResult.resultItem = recipe.cGradeChunk; // C등급 청크
                    bestResult.grade = "C";
                }
                else // 그 외 점수는 F등급 부산물
                {
                    bestResult.success = false;
                    bestResult.message = "조합 실패... 부산물이 생성되었습니다.";
                    bestResult.resultItem = recipe.failureByproduct; // 레시피별 고유 부산물
                    bestResult.grade = "F";
                }
            }
        }

        // 3. 결과 아이템을 인벤토리에 추가합니다.
        if (bestResult.resultItem != null)
        {
            if (bestResult.resultItem is ChunkItemData chunk)
            {
                // 결과가 청크 아이템이면 청크 인벤토리로
                PlayerInventory.Instance.AddChunkItem(chunk);
            }
            else
            {
                // 결과가 부산물(일반 Item)이면 핫바로 (혹은 나중에 만들 주 인벤토리로)
                PlayerInventory.Instance.AddItemToHotbar(bestResult.resultItem, 1);
            }
        }

        // 4. UIManager에 최종 결과를 이벤트로 발행합니다.
        OnCombinationAttempted?.Invoke(bestResult);
    }

    /// <summary>
    /// GDD 3.3.1 - 조합 유사도를 계산합니다. (GDD 1.5.0 - 숫자야구/원소조합)
    /// </summary>
    /// <returns>유사도 점수 (0.0f ~ 1.0f)</returns>
    private float CalculateCombinationScore(List<ItemData> input, List<ItemData> recipe)
    {
        // TODO: GDD 1.5.0의 '숫자 야구' 또는 '원소 조합' 방식 구체화 필요
        
        // (임시) 단순 개수 및 포함 여부 체크 로직 (GDD 1.5.1)
        if (recipe == null || recipe.Count == 0) return 0f;

        if (input.Count != recipe.Count) return 0.1f; // 개수만 틀리면 10% 점수

        int correctElements = 0;
        for (int i = 0; i < recipe.Count; i++)
        {
            // (임시) 순서까지 정확히 맞아야 함
            if (input[i] == recipe[i])
            {
                correctElements++;
            }
        }

        return (float)correctElements / recipe.Count; // 1.0f (S등급) ~ 0.0f (F등급)
    }
}