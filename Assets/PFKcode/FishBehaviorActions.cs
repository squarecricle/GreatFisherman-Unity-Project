using UnityEngine;
using System.Collections.Generic; // 我们会用到列表List
using System.Collections;
// [System.Serializable] 是一个“魔法标签”，它告诉Unity：
// “请把这个类的实例（以及它下面的派生类）显示在Inspector窗口中，并帮我保存它们的数据。”
[System.Serializable]
public abstract class FishAction 
{
    // 我们为所有行为定义了一个“执行”的规范。
    // 每个继承它的具体行为，都必须自己实现这个“如何执行”的协程。
    // 我们将 FishController 自身作为参数传进去，方便行为在执行时获取鱼的状态或操作鱼。
    public abstract IEnumerator Execute(FishController controller);
}


// --- 以下是我们可以使用的具体“行为积木” ---


[System.Serializable]
public class Move_Action : FishAction
{
    [Tooltip("这个移动行为持续多少秒")]
    public float Duration = 2f;

    public override IEnumerator Execute(FishController controller)
    {
        float timer = 0f;
        // 目标点现在在行为开始时才计算
        float targetY = Random.Range(controller.MinY, controller.MaxY);
        Vector2 targetPos = new Vector2(controller.RectTransform.anchoredPosition.x, targetY);

        while (timer < Duration)
        {
            controller.RectTransform.anchoredPosition = Vector2.MoveTowards(
                controller.RectTransform.anchoredPosition,
                targetPos,
                controller.CurrentSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }
    } 
}

[System.Serializable]
public class Wait_Action : FishAction
{
    [Tooltip("在原地等待多少秒")]
    public float Duration = 1.5f;

    public override IEnumerator Execute(FishController controller)
    {
        // 等待行为的逻辑非常简单
        yield return new WaitForSeconds(Duration);
    }
}

[System.Serializable]
public class Jump_Action : FishAction
{
    [Tooltip("瞬移后，最短/最长停顿多少秒")]
    public Vector2 PauseDurationRange = new Vector2(0.5f, 1.5f);

    public override IEnumerator Execute(FishController controller)
    {
        float pauseTime = Random.Range(PauseDurationRange.x, PauseDurationRange.y);
        yield return new WaitForSeconds(pauseTime);

        float targetY = Random.Range(controller.MinY, controller.MaxY);
        controller.RectTransform.anchoredPosition = new Vector2(controller.RectTransform.anchoredPosition.x, targetY);
    }
}

[System.Serializable]
public class ChangeSpeed_Action : FishAction
{
    [Tooltip("将鱼的基础速度变更为这个新值")]
    public float NewSpeed = 300f;

    public override IEnumerator Execute(FishController controller)
    {
        // 改变速度是瞬间完成的
        controller.CurrentSpeed = NewSpeed;
        yield return null; // 等待一帧以确保行为序列正常推进
    }
}
[System.Serializable]
public class Jitter_Action : FishAction
{
    [Tooltip("整个抖动行为持续的总时长（秒）")]
    public float Duration = 2f;

    // ---【新参数，取代了旧的Magnitude和Interval】---
    [Tooltip("每次向上或向下移动的最小/最大距离。X为最小值，Y为最大值。")]
    public Vector2 MinMaxMoveDistance = new Vector2(75f, 150f);

    public override IEnumerator Execute(FishController controller)
    {
        float durationTimer = 0f;// 这个计时器用来追踪整个行为的持续时间
        var rectTransform = controller.RectTransform;// 获取鱼的RectTransform组件，方便后续操作

        // 步骤1：初始化方向。
        // 如果鱼的当前位置在中心点(y=0)的下方，则第一次移动方向为上；反之为下。
        // 这确保了初始移动总是趋向于中心区域，表现更自然。
        bool isMovingUp = rectTransform.anchoredPosition.y < 0;

        // 步骤2：根据初始方向，计算出第一个目标点。
        float moveDistance = Random.Range(MinMaxMoveDistance.x, MinMaxMoveDistance.y);
        float targetY = rectTransform.anchoredPosition.y + (isMovingUp ? moveDistance : -moveDistance);
        targetY = Mathf.Clamp(targetY, controller.MinY, controller.MaxY);

        // --- 核心移动循环 ---
        while (durationTimer < Duration)
        {
            durationTimer += Time.deltaTime;// 累加总时长计时器

            // 步骤3：每帧都向当前的目标点平滑移动。
            Vector2 currentPos = rectTransform.anchoredPosition;
            Vector2 targetPos = new Vector2(currentPos.x, targetY);
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, controller.CurrentSpeed * Time.deltaTime);
            rectTransform.anchoredPosition = newPos;

            // 步骤4：【无停顿机制的核心】检查是否已“几乎”到达目标点。
            // 使用Mathf.Approximately可以避免浮点数精度问题。
            if (Mathf.Approximately(newPos.y, targetY))
            {
                // a. 到达后，立即反转下一次的移动方向
                isMovingUp = !isMovingUp;

                // b. 立即计算出一个位于新方向上的、新的目标点
                moveDistance = Random.Range(MinMaxMoveDistance.x, MinMaxMoveDistance.y);
                targetY = rectTransform.anchoredPosition.y + (isMovingUp ? moveDistance : -moveDistance);
                targetY = Mathf.Clamp(targetY, controller.MinY, controller.MaxY);
                
                // 协程会在此处结束当前帧，并在下一帧无缝地朝新目标点移动。
            }

            yield return null;
        }
    }
}
