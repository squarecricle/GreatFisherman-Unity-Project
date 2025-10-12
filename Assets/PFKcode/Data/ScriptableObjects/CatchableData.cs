using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 所有可捕获物品的基础数据类，使用ScriptableObject创建
/// </summary>
public abstract class CatchableData : ScriptableObject
{
    [Header("核心信息")]
    public string ItemName;
    [TextArea] public string Description;
    public Sprite ItemIcon;
    [Tooltip("物品的基础售价，单位为金币")]
    public int BasePrice = 10;
    [Header("产出权重")]
    [Tooltip("该物品在奖池中的基础权重值，越高越常见")]
    public int BaseWeight = 100;
    public enum ItemCategory { Fish, Trash, Material, Rod }
    [Tooltip("此物品的分类")]
    public ItemCategory category;

    [Header("迷你游戏行为参数")]
    [Tooltip("该渔获物品在迷你游戏中的初始位置，0为最底部，1为最顶部")]
    [Range(0, 1)]
    public float InitialNormalizedPosition = 0.5f; // 默认值设为0.5，保持旧行为
    [Tooltip("物品的基础移动速度")]
    public float BaseMoveSpeed = 150f;

    [SerializeReference]
    [Tooltip("物品在“冷静”状态下循环执行的行为序列")]
    public List<FishAction> CalmBehaviorSequence;

    [SerializeReference]
    [Tooltip("物品在“挣扎”状态下循环执行的行为序列")]
    public List<FishAction> StruggleBehaviorSequence;

    [Tooltip("当进度条的值超过这个百分比时，进入“挣扎”状态")]
    [Range(0, 1)]
    public float StruggleThreshold = 0.7f;
}