using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Singularity/Data/Resource/Recipe")]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public class ItemAmount
    {
        public ItemData item;
        public int amount;
    }

    public ItemAmount[] craftingRequirements;
    public ItemAmount[] results;

    //public MachineData requiredMachine;
    public float craftingTime = 1f;
}
