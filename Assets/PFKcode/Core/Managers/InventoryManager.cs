using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// 负责管理玩家的库存所有物品（包括鱼、垃圾、材料、钓竿），包括添加物品、出售物品等功能。
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 使用Dictionary来存储物品，Key是物品的唯一ID，Value(InventoryItem)是物品实例
    private Dictionary<string, InventoryItem> _items = new Dictionary<string, InventoryItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 向背包中添加物品的核心方法
    /// </summary>
    public void AddItem(FishingMiniGameManager.CatchResult catchResult)
    {
        // 1. 判断渔获是鱼还是其他可堆叠物品 (如垃圾)
        bool isFish = catchResult.FishedData.category == CatchableData.ItemCategory.Fish;

        string key;
        InventoryItem itemToAdd;

        if (isFish)
        {
            // 2. 如果是鱼，鱼是“独特物品”，每一条都不同。
            //    - Key: 使用全局唯一ID (GUID) 作为Key，确保字典中每一条鱼都是独立条目。
            //    - Value: 创建一个新的 FishInventoryItem 实例，它包含了品质和长度等独特信息。
            key = System.Guid.NewGuid().ToString();// 生成一个新的唯一ID
            itemToAdd = new FishInventoryItem(catchResult.FishedData, catchResult.FishedQuality, catchResult.Length);// 根据捕获结果创建新的鱼实例
            _items.Add(key, itemToAdd);// 添加key和实例到字典
            Debug.Log($"【InventoryManager】添加了新的鱼: {itemToAdd.sourceData.ItemName} (品质: {(itemToAdd as FishInventoryItem).quality}), Key: {key}");
        }
        else
        {
            // 3. 如果是垃圾等“可堆叠物品”。
            //    - Key: 使用其 ScriptableObject 的名字作为Key。同一种垃圾的Key是相同的。
            key = catchResult.FishedData.name;
            
            // 4. 检查这种物品是否已存在
            if (_items.ContainsKey(key))
            {
                // 5. 如果已存在，则数量+1
                _items[key].quantity++;
                Debug.Log($"【InventoryManager】增加了物品: {_items[key].sourceData.ItemName} 的数量, 当前数量: {_items[key].quantity}");
            }
            else
            {
                // 6. 如果不存在，则创建新的 InventoryItem 实例并存入字典
                itemToAdd = new InventoryItem(catchResult.FishedData);
                _items.Add(key, itemToAdd);
                Debug.Log($"【InventoryManager】添加了新种类的物品: {itemToAdd.sourceData.ItemName}, Key: {key}");
            }
        }
    }

    /// <summary>
    /// 出售所有物品
    /// </summary>
    /// <returns>返回售出的总金额</returns>
    public int SellAllItems()
    {
        // 1. 遍历字典，累加所有物品的Price
        // 我们使用了Linq的Sum方法，这是一个更简洁的写法
        // item.Value.Price 会自动根据是鱼还是普通物品，调用其正确的价格计算逻辑
        int totalValue = _items.Sum(item => item.Value.Price);

        Debug.Log($"【InventoryManager】售出了 {_items.Count} 格物品, 总价值: {totalValue} 金币。");

        // 2. 清空字典
        _items.Clear();

        // 3. 返回总金额
        return totalValue;
    }
        
    /// <summary>
    /// 根据物品分类值
    /// 返回所有属于该分类的物品列表,目前有四种列表：鱼、垃圾、材料、钓竿
    /// </summary>
    public List<InventoryItem> GetItemsByCategory(CatchableData.ItemCategory category)
    {
        // 1. 创建一个新的List<InventoryItem>
        List<InventoryItem> foundItems = new List<InventoryItem>();
        
        // 2. 遍历_items字典
        foreach (var item in _items.Values)
        {
            // 3. 如果item.sourceData.category == category，则将其加入新List
            if (item.sourceData.category == category)
            {
                foundItems.Add(item);
            }
        }
        
        Debug.Log($"【InventoryManager】查询分类 {category}, 找到了 {foundItems.Count} 个物品。");
        // 4. 返回这个新List
        return foundItems;
    }
}