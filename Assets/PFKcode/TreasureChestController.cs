using System.Collections.Generic;
using UnityEngine;

public class TreasureChestController : MonoBehaviour
{
    [Header("宝箱配置")]
    [Tooltip("将所有可能掉落的宝箱资产（普通、豪华等）都拖到这里")]
    public List<TreasureChestData> AvailableChests;

    /// <summary>
    /// 尝试根据概率奖励一个宝箱。这是给 FishingMiniGameManager 调用的核心方法。
    /// </summary>
    public void TryToAwardChest()
    {
        // 遍历所有可用的宝箱类型
        foreach (var chestData in AvailableChests)
        {
            // 为每个宝箱进行一次“roll点”
            if (Random.Range(0f, 1f) < chestData.DropChance)
            {
                // 如果 roll点成功，则“获得”这个宝箱
                Debug.Log($"恭喜！额外获得了宝箱：{chestData.ItemName}!");

                // --- 临时的开箱逻辑 ---
                // 随机一个物品数量
                int lootCount = Random.Range(chestData.LootCountRange.x, chestData.LootCountRange.y + 1);
                Debug.Log($"宝箱里有 {lootCount} 件物品：");

                // 从宝箱自己的奖池里随机挑选物品
                for (int i = 0; i < lootCount; i++)
                {
                    if (chestData.LootPool != null && chestData.LootPool.Count > 0)
                    {
                        var randomItem = chestData.LootPool[Random.Range(0, chestData.LootPool.Count)];
                        Debug.Log($"  - {randomItem.ItemName}");
                    }
                }

                // 重要：一旦获得了一个宝箱，就立刻停止后续的判定，避免一次钓鱼获得多个宝箱
                return;
            }
        }
    }
}