using System.Collections.Generic; // 引入命名空间，以便使用 List<>
using UnityEngine;

/// <summary>
/// 钓鱼点管理器
/// 负责管理该钓鱼点的“鱼池”以及启动钓鱼迷你游戏的逻辑
/// </summary>
public class FishingSpot : MonoBehaviour
{
    [Header("系统关联")]
    [Tooltip("场景中唯一的 FishingGameManager 对象")]
    public FishingGameManager fishingGameManager;

    [Header("鱼池配置")]
    [Tooltip("所有可能在这个钓鱼点钓到的鱼的数据列表")]
    public List<FishData> availableFish;

    // 这个方法将是我们未来从外部（比如玩家控制器）调用的入口
    //检查错误
    //选择鱼
    //启动钓鱼游戏
    public void StartFishing()
    {
        // 检查1：确保 fishingGameManager 已经关联
        if (fishingGameManager == null)
        {
            Debug.LogError("FishingSpot 错误: FishingGameManager 未关联!");
            return;
        }

        // 检查2：确保鱼池里有鱼
        if (availableFish == null || availableFish.Count == 0)
        {
            Debug.LogError("FishingSpot 错误: 鱼池 (availableFish) 为空!");
            return;
        }

        // --- 核心产出逻辑 ---
        // V1.0 - 简单的纯随机选择
        // 从 availableFish 列表中随机选择一个索引
        int randomIndex = Random.Range(0, availableFish.Count);
        FishData selectedFish = availableFish[randomIndex];

        // 将选中的鱼的数据，传递给 FishingGameManager
        fishingGameManager.currentFishData = selectedFish;

        // 命令 FishingGameManager 开始游戏
        fishingGameManager.StartGame();

        Debug.Log($"一条 {selectedFish.fishName} 上钩了!");
    }
}