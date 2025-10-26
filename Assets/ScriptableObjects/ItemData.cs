using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Singularity/Data/Resource/Item")]

[System.Serializable]

public class ItemData : ResourceData
{
    public string itemID;
    public int stackLimit = 99;
    public bool isConsumable = false;
    public Rarity rarity;

    private void OnValidate()
    {
        if (stackLimit < 1)
        {
            stackLimit = 99;
        }

    }

}