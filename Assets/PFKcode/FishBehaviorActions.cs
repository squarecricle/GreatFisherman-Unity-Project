using UnityEngine;
using System.Collections.Generic; // 我们会用到列表List

// [System.Serializable] 是一个“魔法标签”，它告诉Unity：
// “请把这个类的实例（以及它下面的派生类）显示在Inspector窗口中，并帮我保存它们的数据。”
[System.Serializable]
public abstract class FishAction 
{
    // 这里可以放所有行为都共有的参数，但为了保持简洁，我们暂时留空。
    // abstract 关键字意味着 FishAction 本身只是一个“概念”或“模板”，不能被直接创建，
    // 只有继承它的具体行为（如MoveAction）才能被创建。
}


// --- 以下是我们可以使用的具体“行为积木” ---


[System.Serializable]
public class Move_Action : FishAction
{
    [Tooltip("鱼的移动速度")]
    public float Speed = 150f;
    [Tooltip("这个移动行为持续多少秒")]
    public float Duration = 2f;
}

[System.Serializable]
public class Wait_Action : FishAction
{
    [Tooltip("在原地等待多少秒")]
    public float Duration = 1.5f;
}

[System.Serializable]
public class Jump_Action : FishAction
{
    [Tooltip("瞬移后，最短/最长停顿多少秒")]
    public Vector2 PauseDurationRange = new Vector2(0.5f, 1.5f);
}

[System.Serializable]
public class ChangeSpeed_Action : FishAction
{
    [Tooltip("将鱼的基础速度变更为这个新值")]
    public float NewSpeed = 300f;
}