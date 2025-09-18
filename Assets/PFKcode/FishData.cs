using UnityEngine;

// [CreateAssetMenu] 是一个非常强大的特性（Attribute）。
// 它告诉Unity编辑器：“请在 Assets/Create 菜单中添加一个创建此类型资产的选项。”
// 这样，我们就可以像创建材质球或动画控制器一样，在项目文件夹中直接创建“鱼”的数据文件了。
[CreateAssetMenu(fileName = "NewFishData", menuName = "PocketFishingKing/Fish Data")]
public class FishData : ScriptableObject // 注意：这里继承的是 ScriptableObject，而不是 MonoBehaviour
{
    [Header("核心信息")]
    public string fishName; // 鱼的名字
    [TextArea] public string description; // 鱼的描述
    public Sprite fishIcon; // 鱼的图标，用于UI展示

    // 使用枚举(enum)来预定义所有可能的选项，可以防止拼写错误，并且在Inspector中会显示为方便的下拉菜单。
    public enum FishRarity { 普通, 稀有, 史诗, 传说 }
    public FishRarity rarity; // 稀有度

    public enum FishingLocation { 小溪, 森林湖, 公厕马桶, 海滩 }
    [Header("出现条件")]
    public FishingLocation location; // 主要出没地点

    // 使用[Flags]特性可以让枚举在Inspector中表现为可以多选的复选框，非常方便！
    [System.Flags]
    public enum TimeOfDay { 无 = 0, 白天 = 1, 夜晚 = 2 }
    public TimeOfDay timeOfDay; // 出没时间（可以同时选白天和夜晚）

    [System.Flags] // <--- 第一步：添加Flags特性
    public enum FishBehaviorType 
    {
        // 第二步：将值设为2的幂，这是Flags枚举工作的关键
        平滑移动 = 1,     // 2^0
        位置跳跃 = 2,     // 2^1
        状态切换 = 4,     // 2^2
        随机抖动 = 8      // 2^3
    }
    [Header("迷你游戏行为参数")]  
    public FishBehaviorType behaviorType; // 鱼的核心行为模式
    public float moveSpeed = 150f; // 基础移动速度
    public float minPauseDuration = 0.5f; // 停顿时长的最小值
    public float maxPauseDuration = 1.5f; // 停顿时长的最大值
    
    // 更多行为参数可以根据需要添加，比如跳跃距离，快速状态下的速度倍率等
    // V1.0 我们先用以上通用参数，未来可以轻松扩展

    [Header("产出信息")]
    public int baseSellPrice; // 基础售价

    // 我们用Vector2来巧妙地存储一个(最小值, 最大值)的范围，x是min，y是max。
    [Tooltip("鱼的长度范围(厘米)，X为最小值, Y为最大值")]
    public Vector2 lengthRangeMianQiang; // 勉强上钩品质的长度范围
    public Vector2 lengthRangeXiangMoXiangYang; // 像模像样
    public Vector2 lengthRangeShiShiDuiJue; // 史诗对决
    public Vector2 lengthRangeChuiNiuZiBen; // 吹牛资本
}