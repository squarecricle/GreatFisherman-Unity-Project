using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // 使用Dictionary来存储物品，Key是物品的唯一ID，Value是物品实例
    private Dictionary<string, InventoryItem> _items = new Dictionary<string, InventoryItem>();

    /// <summary>
    /// 向背包中添加物品的核心方法
    /// </summary>
    public void AddItem(FishingMiniGameManager.CatchResult catchResult)
    {
        // TODO: 阶段二实现
        // 1. 判断渔获是鱼还是垃圾
        // 2. 生成Key（垃圾用SO的ID，鱼用唯一ID）
        // 3. 检查Key是否存在
        // 4. 如果是可堆叠物品且已存在，则quantity++
        // 5. 如果不存在，则创建新的InventoryItem或FishInventoryItem实例并存入字典
        Debug.Log($"【InventoryManager】接收到物品: {catchResult.FishedData.ItemName}, 品质: {catchResult.FishedQuality}, 长度: {catchResult.Length:F2}");
    }

    /// <summary>
    /// 出售所有物品
    /// </summary>
    /// <returns>返回售出的总金额</returns>
    public int SellAllItems()
    {
        // TODO: 阶段二实现
        // 1. 遍历字典，累加所有物品的Price
        // 2. 清空字典 _items.Clear()
        // 3. 返回总金额
        Debug.Log("【InventoryManager】执行了SellAllItems方法。");
        return 100; // 暂时返回一个假数据
    }
        /// <summary>
    /// 根据分类获取物品列表
    /// </summary>
    public List<InventoryItem> GetItemsByCategory(CatchableData.ItemCategory category)
    {
        // TODO: 阶段二实现
        // 1. 创建一个新的List<InventoryItem>
        // 2. 遍历_items字典
        // 3. 如果item.sourceData.category == category，则将其加入新List
        // 4. 返回这个新List
        Debug.Log($"【InventoryManager】请求获取分类为 {category} 的物品。");
        return new List<InventoryItem>(); // 返回一个空的列表
    }
}