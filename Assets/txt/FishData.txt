// FishData.cs (修改后)
using UnityEngine;

// 我们保留CreateAssetMenu，但路径可以更具体
[CreateAssetMenu(fileName = "NewFishData", menuName = "PocketFishingKing/Fish Data")]
// 注意这里的变化，它现在继承自我们新的基类！
public class FishData : CatchableData 
{
    [Header("鱼类专属信息")] // 为了区分，我们可以加一个新的Header
    public FishRarity Rarity;
    public enum FishRarity { 普通, 稀有, 史诗, 传说 }


    [Header("出现条件")]
    public FishingLocation Location;
    public enum FishingLocation { 小溪, 森林湖, 公厕马桶, 海滩 }

    [System.Flags]
    public enum TimeOfDay { 无 = 0, 白天 = 1, 夜晚 = 2 }
    public TimeOfDay ApplicableTimeOfDay;

    [Header("产出信息")]
    public int BaseSellPrice; 
    [Tooltip("鱼的长度范围(厘米)，X为最小值, Y为最大值")]
    public Vector2 LengthRangeMianQiang;
    public Vector2 LengthRangeShiShiDuiJue;
    public Vector2 LengthRangeXiangMoXiangYang;
    public Vector2 LengthRangeChuiNiuZiBen;

    // 注意：所有关于移动、行为序列的属性都消失了，因为它们被“遗传”了！
}