// TrashData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrashData", menuName = "PocketFishingKing/Trash Data")]
public class TrashData : CatchableData
{
    [Header("稀有度")] // 为了区分，我们可以加一个新的Header
    public FishRarity Rarity;
    public enum FishRarity { 普通, 稀有, 史诗, 传说 }
    // 它所有的核心数据（名字、图标、行为、权重）都继承自 CatchableData。
    // 未来如果需要，比如想给垃圾添加“可分解出的材料”列表，就可以在这里添加。
}