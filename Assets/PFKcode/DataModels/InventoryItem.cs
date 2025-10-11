using UnityEngine;

// 我们使用 [System.Serializable] 让Unity能够序列化（保存）这个类的数据。
[System.Serializable] 
public class InventoryItem   // 背包中的单格物品实例
{
    // 对应ScriptableObject的数据模板，用于查找物品的通用信息（名字、图标等）
    public CatchableData sourceData; 
    
    // 物品数量
    public int quantity;

    // 一个只读的、动态计算价格的属性。
    // virtual关键字允许子类（比如FishInventoryItem）重写这个计算逻辑。
    public virtual int Price 
    {
        get 
        {
            if (sourceData == null) return 0;
            // 基础计算逻辑：物品基础售价 * 数量
            // TODO: 未来可以加入更多价格计算因素
            return sourceData.BasePrice * quantity;
        }
    }

    // 构造函数，方便我们快速创建实例
    public InventoryItem(CatchableData source)
    {
        this.sourceData = source;
        this.quantity = 1;
    }
}

// 鱼的专属实例数据，继承自InventoryItem
[System.Serializable]
public class FishInventoryItem : InventoryItem
{
    public FishingMiniGameManager.FishQuality quality;
    public float length;

    // 重写（override）基类的Price属性，加入品质对价格的影响
    public override int Price
    {
        get
        {
            if (sourceData == null) return 0;
            
            float qualityMultiplier = 1.0f; // 品质的价格倍率
            switch (quality)
            {
                case FishingMiniGameManager.FishQuality.吹牛资本:
                    qualityMultiplier = 2.0f;
                    break;
                case FishingMiniGameManager.FishQuality.史诗对决:
                    qualityMultiplier = 1.5f;
                    break;
                case FishingMiniGameManager.FishQuality.像模像样:
                    qualityMultiplier = 1.0f;
                    break;
                case FishingMiniGameManager.FishQuality.勉强上钩:
                    qualityMultiplier = 0.8f;
                    break;
            }

            // 最终价格 = 基础售价 * 品质倍率 * 数量
            int finalPrice = Mathf.RoundToInt(sourceData.BasePrice * qualityMultiplier * quantity);
            return finalPrice;
        }
    }

    // 子类的构造函数
    public FishInventoryItem(CatchableData source, FishingMiniGameManager.FishQuality quality, float length) : base(source)
    {
        this.quality = quality;
        this.length = length;
    }
}