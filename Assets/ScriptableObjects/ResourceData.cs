using UnityEngine;

public abstract class ResourceData : ScriptableObject
{
    [Header("Common Resource Information")]
    public string resourceName;
    public Sprite icon;
    [TextArea]
    public string description;
}
