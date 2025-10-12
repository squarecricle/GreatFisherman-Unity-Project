using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FishingSpot : MonoBehaviour
{
    [Header("系统关联")]
    [SerializeField, FormerlySerializedAs("FishingGameManager")] private FishingMiniGameManager fishingGameManager;
    [SerializeField, FormerlySerializedAs("StartFishingButton")] private GameObject startFishingButton;
    [SerializeField, FormerlySerializedAs("CastingController")] private CastingAndHookingController castingController;

    [Header("奖池配置")]
    // --- 【修改1】: 列表类型和名字都变了 ---
    [SerializeField, FormerlySerializedAs("LootPool")] private List<CatchableData> lootPool; // 不再是 FishPool，而是通用的 LootPool

    public void StartFishing()
    {
        // 检查必要的组件是否都已关联
        if (castingController == null || lootPool == null || lootPool.Count == 0)
        {
            Debug.LogError("FishingSpot 配置不完整! 请检查 CastingController 和 LootPool。");
            return;
        }

        // 隐藏“开始钓鱼”按钮
        startFishingButton.SetActive(false);
        // --- 核心产出逻辑修改 ---
        CatchableData selectedItem = SelectItemByWeight(); // 调用我们新的方法
        if (selectedItem == null)
        {
            Debug.LogError("未能根据权重选出任何物品！请检查奖池配置。");
            OnFishingSessionEnd();
            return;
        }
        
        // 我们需要判断钓上来的具体是鱼还是什么
        if (selectedItem is FishData)
        {
            // 如果是鱼，才传递给GameManager
            fishingGameManager.CurrentCatchableData = selectedItem;
            Debug.Log($"一条 {(selectedItem as FishData).Rarity} 品质的鱼 '{selectedItem.ItemName}' 准备上钩!");
        }
        else
        {
            // 如果是垃圾或其他东西，我们暂时先清空GameManager中的鱼数据
            // 未来这里可以用来传递垃圾的数据
            fishingGameManager.CurrentCatchableData = selectedItem;
            Debug.Log($"一个 '{selectedItem.ItemName}' 准备上钩!");
        }
        
        // 2. 开始抛竿过程
        castingController.StartCastingProcess();
    }

    // --- 【修改2】: 整个权重挑选方法被重写，变得更通用 ---
    /// <summary>
    /// 根据物品的 BaseWeight，从 LootPool 列表中挑选一个物品
    /// </summary>
    /// <returns>被选中的物品</returns>
    public CatchableData SelectItemByWeight()
    {
        // --- 步骤 a: 计算总权重 ---
        int totalWeight = 0;
        foreach (CatchableData item in lootPool)
        {
            totalWeight += item.BaseWeight; // 直接使用基类里的BaseWeight
        }

        if (totalWeight == 0) return null;

        // --- 步骤 b: 生成一个0到总权重之间的随机数 ---
        int randomWeight = Random.Range(0, totalWeight);
        
        // --- 步骤 c: 遍历所有物品，看随机数落入哪个区间 ---
    foreach (CatchableData item in lootPool)
        {
            // 如果随机数小于当前物品的权重，就选中这个物品
            if (randomWeight < item.BaseWeight)
            {
                return item;
            }
            
            // 如果没选中，就从随机数中减去当前物品的权重，继续下一轮
            randomWeight -= item.BaseWeight;
        }

        return null; // 理论上不应该执行到这里
    }
    
    // 我们不再需要 GetWeightForRarity 这个方法了，可以删掉它。

    // 当钓鱼环节结束时调用，让开始按钮重新出现
    public void OnFishingSessionEnd()
    {
        startFishingButton.SetActive(true);
    }
}