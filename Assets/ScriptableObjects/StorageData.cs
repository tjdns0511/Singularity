using UnityEngine;

public enum StorageType { Item, Liquid }

[CreateAssetMenu(fileName = "New Storage", menuName = "Singularity/Data/Block/Storage")]

public class StorageData : BlockData
{
    [Header("Storage Infomation")]
    public BlockDirection Direction = BlockDirection.South;
    public StorageType StorageType;
}
