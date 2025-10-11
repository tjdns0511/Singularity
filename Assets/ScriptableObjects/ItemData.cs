using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Singularity/Data/Resource/Item")]

[System.Serializable]
public class CraftingRequirement
{
    public ItemData item;
    public int amount;
}
public class ItemData : ResourceData
{
    public int stackLimit = 99;
    public bool isConsumable = false;
    public Rarity rarity;
    public CraftingRequirement[] craftRequirements;

    private void OnValidate()
    {
        if (stackLimit < 1)
        {
            stackLimit = 99;
        }

    }

}