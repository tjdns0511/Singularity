// In Assets/ScriptableObjects/MachineData.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GDD 4.4.2 - '기계' 타입의 블록 데이터입니다. (예: 채굴기, 조립기, 용광로)
/// BlockData를 상속받습니다.
/// </summary>
[CreateAssetMenu(fileName = "NewMachineData", menuName = "Singularity/Data/Machine Data")]
public class MachineData : BlockData
{
    [Header("Machine Properties")]
    [Tooltip("기계 처리 속도 (예: 1.0 = 기본 속도)")]
    public float processingSpeed = 1.0f;

    // (GDD 3.5.2 / 7단계)
    // [Tooltip("이 기계가 사용하는 제작 레시피 (조립기/용광로용)")]
    // public List<RecipeData> availableRecipes; 
}