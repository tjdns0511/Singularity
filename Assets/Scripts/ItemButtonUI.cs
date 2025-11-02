using System;
using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용
using TMPro; // TextMeshProUGUI 컴포넌트 사용

/// <summary>
/// UI 버튼에 아이템(BuildingItemData 포함)의 정보를 표시하는 스크립트입니다.
/// 버튼 프리팹에 이 스크립트를 추가하고, 하위 UI 요소들을 연결해주세요.
/// </summary>
public class ItemButtonUI : MonoBehaviour
{
    [Header("UI Element References")]
    [Tooltip("아이템 아이콘을 표시할 Image 컴포넌트")]
    [SerializeField] private Image itemIcon;

    [Tooltip("아이템 이름을 표시할 TextMeshProUGUI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("아이템 수량을 표시할 TextMeshProUGUI 컴포넌트 (없어도 됨)")]
    [SerializeField] private TextMeshProUGUI itemQuantityText;

    /// <summary>
    /// ItemData(BuildingItemData 포함)와 수량으로 버튼 UI를 설정합니다.
    /// </summary>
    /// <param name="itemData">표시할 아이템 데이터 (BuildingItemData 가능)</param>
    /// <param name="quantity">표시할 수량 (-1 이거나 없으면 수량 텍스트 숨김)</param>
    public void Setup(ItemData itemData, int quantity = -1)
    {
        if (itemData == null)
        {
            Debug.LogWarning("Setup called with null ItemData.", this.gameObject);
            string displayLabel = itemData.resourceName;
            if (string.IsNullOrWhiteSpace(displayLabel))
            {
                displayLabel = !string.IsNullOrWhiteSpace(itemData.displayName)
                    ? itemData.displayName
                    : itemData.name;
            }
            itemNameText.text = displayLabel;
            if (itemIcon != null) itemIcon.enabled = false;
            if (itemNameText != null) itemNameText.text = "???";
            if (itemQuantityText != null) itemQuantityText.gameObject.SetActive(false);
            return;
        }

        // 아이콘 설정 (ItemData에 icon 필드가 있다고 가정)
        if (itemIcon != null)
        {
            if (itemData.icon != null)
            {
                itemIcon.sprite = itemData.icon;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = false; // 아이콘 없으면 비활성화
                Debug.LogWarning($"ItemData '{itemData.name}' is missing an icon.", this.gameObject);
            }
        }

        // 이름 설정 (ItemData에 itemName 필드가 있다고 가정)
        if (itemNameText != null)
        {
            itemNameText.text = itemData.resourceName;
        }

        // 수량 설정
        if (itemQuantityText != null)
        {
            // BuildingItemData는 보통 수량 표시가 필요 없을 수 있음 (선택 사항)
            // bool showQuantity = quantity > 0 && !(itemData is BuildingItemData); // 건물 아이템이면 수량 숨기기
            bool showQuantity = quantity > 0; // 일단 모든 아이템 수량 표시
            itemQuantityText.gameObject.SetActive(showQuantity);
            if (showQuantity)
            {
                itemQuantityText.text = $"x{quantity}";
            }
        }
    }

    // BlockData를 받는 Setup 함수는 더 이상 필요 없으므로 삭제합니다.
    // public void Setup(BlockData blockData) { ... }
}