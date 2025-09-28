using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewFishData", menuName = "PocketFishingKing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("核心信息")]
    public string FishName;
    [TextArea] public string Description; 
    public Sprite FishIcon;
    public enum FishRarity { 普通, 稀有, 史诗, 传说 } 
    public FishRarity Rarity;

    public enum FishingLocation { 小溪, 森林湖, 公厕马桶, 海滩 }
    [Header("出现条件")]
    public FishingLocation Location;
    [System.Flags]
    public enum TimeOfDay { 无 = 0, 白天 = 1, 夜晚 = 2 }
    public TimeOfDay ApplicableTimeOfDay;

    // ---【核心修改部分】---
    [Header("迷你游戏行为参数")]
    [Tooltip("鱼的基础移动速度，可以被'改变速度'行为在游戏中动态修改")]
    public float BaseMoveSpeed = 150f;

    // [SerializeReference] 是另一个“魔法标签”，它告诉Unity的列表：
    // “请允许我存放不同种类的‘行为积木’（Move_Action, Wait_Action等），并正确地保存它们。”
    [SerializeReference]// 允许在列表中存放不同类型的实例
    [Tooltip("鱼在“冷静”状态下循环执行的行为序列")]
    public List<FishAction> CalmBehaviorSequence;

    [SerializeReference]
    [Tooltip("鱼在“挣扎”状态下循环执行的行为序列")]
    public List<FishAction> StruggleBehaviorSequence;
    
    [Tooltip("当进度条的值超过这个百分比时，鱼会进入“挣扎”状态")]
    [Range(0, 1)]
    public float StruggleThreshold = 0.7f;
    // ---【修改结束】---


    [Header("产出信息")]
    public int BaseSellPrice; 
    [Tooltip("鱼的长度范围(厘米)，X为最小值, Y为最大值")]
    public Vector2 LengthRangeMianQiang;
    public Vector2 LengthRangeXiangMoXiangYang;
    public Vector2 LengthRangeShiShiDuiJue;
    public Vector2 LengthRangeChuiNiuZiBen;
}