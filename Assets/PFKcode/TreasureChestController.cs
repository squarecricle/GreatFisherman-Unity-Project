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
    public TreasureChestData TryToAwardChest()
    {
        foreach (var chestData in AvailableChests)
        {
            if (Random.Range(0f, 1f) < chestData.DropChance)
            {
                // Roll点成功，直接返回这个宝箱的数据
                return chestData;
            }
        }
        // 如果遍历完所有宝箱都没有成功，则返回null
        return null;
    }
}