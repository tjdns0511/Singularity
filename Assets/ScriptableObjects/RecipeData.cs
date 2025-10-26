using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Singularity/Data/Resource/Recipe")]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public class ItemAmount
    {
        public ItemData item;
        [Min(1)]
        public int amount = 1;
    }

    [Header("Recipe Information")]
    public string recipeID;

    public List<ItemAmount> craftingRequirements;
    public List<ItemAmount> results;

    public MachineData requiredMachine;
    public float craftingTime = 1f;
}
