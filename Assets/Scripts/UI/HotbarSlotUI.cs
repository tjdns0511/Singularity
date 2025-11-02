// In Assets/Scripts/UI/HotbarSlotUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject highlightBorder; // 선택됐을 때 켤 테두리
    [SerializeField] private TextMeshProUGUI slotNumberText; // '1'~'9' 표시용

    void Start()
    {
        // 아이콘과 텍스트를 비워서 시작
        ClearSlot();
    }

    /// <summary>
    /// 이 슬롯의 UI를 InventorySlot 데이터에 맞게 갱신합니다.
    /// </summary>
    public void UpdateSlot(InventorySlot slot)
    {
        if (slot != null && slot.itemDataRef != null)
        {
            iconImage.enabled = true;
            // iconImage.sprite = slot.itemDataRef.icon; // ItemData에 아이콘 필드 필요

            quantityText.enabled = true;
            quantityText.text = slot.quantity.ToString();
        }
        else
        {
            ClearSlot();
        }
    }

    /// <summary>
    /// 슬롯을 빈 상태로 만듭니다.
    /// </summary>
    public void ClearSlot()
    {
        iconImage.enabled = false;
        iconImage.sprite = null;
        quantityText.enabled = false;
        quantityText.text = "";
    }

    /// <summary>
    /// 이 슬롯이 선택되었는지 여부에 따라 하이라이트 테두리를 켜고 끕니다.
    /// </summary>
    public void SetHighlight(bool isSelected)
    {
        highlightBorder.SetActive(isSelected);
    }
}