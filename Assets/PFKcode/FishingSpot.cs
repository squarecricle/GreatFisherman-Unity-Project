using System.Collections.Generic;
using UnityEngine;

public class FishingSpot : MonoBehaviour
{
    [Header("系统关联")]
    public FishingMiniGameManager FishingGameManager;
    public GameObject StartFishingButton; 
    public CastingAndHookingController CastingController; // 新增对抛竿控制器的引用
    [Header("鱼池配置")]
    public List<FishData> FishPool;

    // 这个方法是我们整个系统的入口
        public void StartFishing()
    {
        // 检查必要的组件是否都已关联
        if (CastingController == null || FishPool == null || FishPool.Count == 0)
        {
            Debug.LogError("FishingSpot 配置不完整! 请检查 CastingController 和 FishPool。");
            return;
        }

        // 隐藏“开始钓鱼”按钮
        StartFishingButton.SetActive(false);

        // --- 核心产出逻辑 (这部分不变) ---
        FishData selectedFish = SelectFishByWeight();
        if (selectedFish == null)
        {
            Debug.LogError("未能根据权重选出任何鱼！请检查鱼池配置。");
            OnFishingSessionEnd(); // 让按钮回来，防止游戏卡住
            return;
        }

        // --- 流程衔接修改 ---
        // 1. 将选中的鱼的数据，传递给 FishingMiniGameManager（它需要提前知道一会要跟谁博弈）
        FishingGameManager.CurrentFishData = selectedFish;

        // 2. 启动“抛竿与上钩”流程，而不是直接启动小游戏
        CastingController.StartCastingProcess();

        Debug.Log($"一条 {selectedFish.Rarity} 品质的鱼 '{selectedFish.FishName}' 准备上钩!");
    }

    /// <summary>
    /// 根据鱼的稀有度权重，从 FishPool 列表中挑选一条鱼
    /// </summary>
    /// <returns>被选中的鱼</returns>
    private FishData SelectFishByWeight()
    {
        // --- 步骤 a: 计算总权重 ---
        int totalWeight = 0;
        foreach (FishData fish in FishPool)
        {
            totalWeight += GetWeightForRarity(fish.Rarity);
        }

        // --- 步骤 b: 生成一个0到总权重之间的随机数 ---
        int randomWeight = Random.Range(0, totalWeight);

        // --- 步骤 c: 遍历所有鱼，看随机数落入哪个区间 ---
        FishData selectedFish = null;
        foreach (FishData fish in FishPool)
        {
            int currentFishWeight = GetWeightForRarity(fish.Rarity);
            
            // 如果随机数小于当前鱼的权重，就选中这条鱼
            if (randomWeight < currentFishWeight)
            {
                selectedFish = fish;
                break; // 找到后立刻跳出循环
            }
            
            // 如果没选中，就从随机数中减去当前鱼的权重，继续下一轮
            randomWeight -= currentFishWeight;
        }

        return selectedFish;
    }

    /// <summary>
    /// 辅助函数：根据稀有度返回一个整数权重值
    /// </summary>
    /// <param name="rarity">鱼的稀有度</param>
    /// <returns>权重值</returns>
    private int GetWeightForRarity(FishData.FishRarity rarity)
    {
        // 这些数值是游戏策划的核心，未来可以随时调整来控制产出概率
        switch (rarity)
        {
            case FishData.FishRarity.普通:   return 100;
            case FishData.FishRarity.稀有:   return 25;
            case FishData.FishRarity.史诗:   return 5;
            case FishData.FishRarity.传说:   return 1;
            default:                        return 0; // 如果有未定义的稀有度，权重为0
        }
    }

    // 当钓鱼环节结束时调用，让开始按钮重新出现
    public void OnFishingSessionEnd()
    {
        StartFishingButton.SetActive(true);
    }
}