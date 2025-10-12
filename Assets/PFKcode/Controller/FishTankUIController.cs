using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using TMPro; // 引入TextMeshPro
using System.Collections.Generic; // 引入List

/// <summary>
/// 控制鱼箱面板的显示与交互。
/// </summary>
public class FishTankUIController : MonoBehaviour
{
    [Header("系统关联")]
    [SerializeField] private InventoryManager inventoryManager; // 引用库存管理器
    [SerializeField] private PlayerWalletManager playerWalletManager; // 引用钱包管理器

    [Header("UI组件")]
    [SerializeField] private GameObject fishItemPrefab; // 用于展示单条鱼的UI预制件
    [SerializeField] private Transform fishListContent; // 鱼列表的容器 (ScrollView的Content)
    [SerializeField] private Button sellAllButton; // “全部出售”按钮
    [SerializeField] private TextMeshProUGUI emptyMessageText; // 当鱼箱为空时显示的文本

    /// <summary>
    /// 初始化并显示鱼箱。由GameFlowManager调用。
    /// </summary>
    /// <param name="canSell">是否允许出售（决定出售按钮是否可见）</param>
    public void Initialize(bool canSell)
    {
        // 根据是否可出售，来显示或隐藏“全部出售”按钮
        sellAllButton.gameObject.SetActive(canSell);
        
        // 刷新界面显示
        RefreshDisplay();
    }

    /// <summary>
    /// 当UI上的【全部出售】按钮被点击时调用
    /// </summary>
    public void OnSellAllButtonClicked()
    {
        // 1. 调用库存管理器的出售方法，并获取总收入
        int totalValue = inventoryManager.SellAllItems();

        // 2. 如果确实卖出了东西，就通知钱包管理器增加金币
        if (totalValue > 0)
        {
            playerWalletManager.AddGold(totalValue);
            Debug.Log($"【FishTankUIController】成功出售所有物品,获得 {totalValue} 金币。");
        }
        else
        {
            Debug.Log("【FishTankUIController】鱼箱里没有东西可卖。");
        }

        // 3. 再次刷新界面（此时应该变空了）
        RefreshDisplay();
    }

    /// <summary>
    /// 刷新整个鱼箱界面的显示
    /// </summary>
    public void RefreshDisplay()
    {
        // 1. 清理旧的列表项
        // 遍历容器的所有子对象并销毁它们，防止重复生成
        foreach (Transform child in fishListContent)
        {
            Destroy(child.gameObject);
        }

        // 2. 从InventoryManager获取所有分类为“鱼”的物品
        List<InventoryItem> fishItems = inventoryManager.GetItemsByCategory(CatchableData.ItemCategory.Fish);

        // 3. 根据获取到的鱼，生成新的UI列表项
        if (fishItems.Count > 0)
        {
            emptyMessageText.gameObject.SetActive(false); // 有鱼，隐藏空消息
            foreach (var item in fishItems)
            {
                // a. 实例化预制件（在鱼列表中（通过预制体）复制一个新的实例）
                GameObject itemGO = Instantiate(fishItemPrefab, fishListContent);

                // b. 获取预制件上的FishTankItemUI脚本（目的是为了显示鱼的详细信息）
                FishTankItemUI itemUI = itemGO.GetComponent<FishTankItemUI>();
                
                // c. 调用Setup方法，传入鱼的实例数据，设置UI显示
                // 我们确定这里的数据都是鱼，所以可以安全地进行(FishInventoryItem)类型转换
                itemUI.Setup(item as FishInventoryItem);
            }
        }
        else
        {
            emptyMessageText.gameObject.SetActive(true); // 没鱼，显示空消息
        }
    }
}