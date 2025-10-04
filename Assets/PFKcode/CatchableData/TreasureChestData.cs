using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTreasureChestData", menuName = "PocketFishingKing/Treasure Chest Data")]
public class TreasureChestData : CatchableData

{
    [Header("宝箱专属信息")]

    [Tooltip("未来可以用于决定宝箱能开出什么等级的物品")]
    public int Tier = 1;
    [Tooltip("当一条鱼被成功钓上时,这个宝箱作为额外奖励出现的概率(0到1之间)")]
    [Range(0, 1)]
    public float DropChance = 0.15f;

    [Tooltip("宝箱能开出的物品数量范围,X为最小值,Y为最大值")]
    public Vector2Int LootCountRange = new Vector2Int(1, 3);

    [Tooltip("宝箱开出的物品列表（目前来源于现有的垃圾）")]
    public List<CatchableData> LootPool;

}