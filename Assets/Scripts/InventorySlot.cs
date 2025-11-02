// In Assets/Scripts/InventorySlot.cs

[System.Serializable] // GDD 4.5.1 - Unity 인스펙터 및 JSON 직렬화를 위함
public class InventorySlot
{
    public ItemData itemDataRef; // GDD 4.5.1  - 어떤 아이템인가
    public int quantity;         // GDD 4.5.1  - 몇 개인가

    public InventorySlot(ItemData item, int amount)
    {
        itemDataRef = item;
        quantity = amount;
    }
}