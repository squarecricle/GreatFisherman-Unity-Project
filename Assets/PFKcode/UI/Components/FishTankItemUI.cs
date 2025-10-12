using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 负责控制“鱼箱中的单个物品”UI元素的显示
/// 这个脚本挂载在 FishItemPrefab 上。
/// </summary>
public class FishTankItemUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText; // 用于显示品质、长度等信息
    [SerializeField] private TextMeshProUGUI itemPriceText;

    /// <summary>
    /// 根据传入的鱼的实例数据，设置UI显示
    /// </summary>
    /// <param name="fishItem">包含鱼的品质、长度等详细信息的实例</param>
    public void Setup(FishInventoryItem fishItem)
    {
        if (fishItem == null || fishItem.sourceData == null)
        {
            Debug.LogError("传入的FishInventoryItem数据无效!");
            return;
        }

        // 设置图标和名字
        itemIcon.sprite = fishItem.sourceData.ItemIcon;
        itemNameText.text = fishItem.sourceData.ItemName;
        
        // 拼接描述文本，包含品质和长度
        itemDescriptionText.text = $"品质: {fishItem.quality} | 长度: {fishItem.length:F2} cm";
        
        // 设置价格
        // fishItem.Price 会自动调用 FishInventoryItem 中重写过的价格计算逻辑
        itemPriceText.text = $"售价: {fishItem.Price} G";
    }
}