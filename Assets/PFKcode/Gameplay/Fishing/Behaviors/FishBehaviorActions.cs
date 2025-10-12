using UnityEngine;
using System.Collections.Generic; // 我们会用到列表List
using System.Collections;
using Unity.VisualScripting;
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
    [Tooltip("这个移动行为连续执行多少次")]
    public int MoveTimes = 1;
    [Tooltip("单次移动的最长持续时间（秒）。如果超时，则强制结束本次移动，开始下一次。")]
    public float MaxDurationPerMove = 1.5f;
    [Tooltip("鱼偏爱范围的归一化坐标(0为底,1为顶)")]
    public Vector2 PreferredNormLocationRange = new Vector2(0.25f, 0.75f); 

    [Tooltip("当鱼在偏爱范围之外时，有多大几率会游回偏爱范围内。1.0表示必然游回。")]
    [Range(0, 1)]
    public float HomingBias = 0.7f; 

    public override IEnumerator Execute(FishController controller)
    {
        // 外层循环：负责执行N次独立的移动
        for (int i = 0; i < MoveTimes; i++)
        {
            float singleMoveTimer = 0f; // 这个计时器用来追踪单次移动的持续时间
            // 步骤1：为本次移动计算出一个目标点 (调用辅助方法)
            float targetY = CalculateNextTargetY(controller);
            Vector2 targetPos = new Vector2(controller.RectTransform.anchoredPosition.x, targetY);

            // 步骤2：内层循环：负责驱动鱼平滑移动到该目标点
            while (singleMoveTimer < MaxDurationPerMove && 
                   !Mathf.Approximately(controller.RectTransform.anchoredPosition.y, targetPos.y))
            {
                singleMoveTimer += Time.deltaTime;// 累加单次移动计时器
                controller.RectTransform.anchoredPosition = Vector2.MoveTowards(
                    controller.RectTransform.anchoredPosition,
                    targetPos,
                    controller.CurrentSpeed * Time.deltaTime);

                yield return null; // 等待下一帧
            }
            // 当这个内层while循环结束，意味着单次移动已完成
        }
    }

    /// <summary>
    /// 辅助方法：根据鱼的当前位置和偏好，计算下一个移动目标点
    /// </summary>
    private float CalculateNextTargetY(FishController controller)
    {
            // --- 【新增】防御性编程：参数验证与修正 ---
    // 1. 确保归一化值在0到1之间
    float normX = Mathf.Clamp01(PreferredNormLocationRange.x);
    float normY = Mathf.Clamp01(PreferredNormLocationRange.y);

    // 2. 确保最小值不大于最大值
    if (normX > normY)
    {
        // 如果用户输反了，我们悄悄地帮他们交换一下
        float temp = normX;
        normX = normY;
        normY = temp;
    }
    // --- 验证结束 ---
        float yMin = controller.FishMinYBoundary;
        float yMax = controller.FishMaxYBoundary;
        float currentY = controller.RectTransform.anchoredPosition.y;

        // 使用Mathf.Lerp进行坐标转换，更简洁安全
        float preferredMinY = Mathf.Lerp(yMin, yMax, PreferredNormLocationRange.x);
        float preferredMaxY = Mathf.Lerp(yMin, yMax, PreferredNormLocationRange.y);

        // 判断鱼当前是否在舒适区内
        bool isInsidePreferredZone = (currentY >= preferredMinY && currentY <= preferredMaxY);

            // 当鱼在舒适区外时，有HomingBias的几率游回舒适区
            if (Random.Range(0f, 1f) < HomingBias)//鱼想去舒适区
            {
                return Random.Range(preferredMinY, preferredMaxY);// 生成一个舒适区内的位置
            }
            else
            {
                // 否则，继续在整个大范围内随机移动
                return Random.Range(yMin, yMax);// 生成一个大范围内的位置
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

        float targetY = Random.Range(controller.FishMinYBoundary, controller.FishMaxYBoundary);
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
    public Vector2 MinMaxJitterDistance = new Vector2(75f, 150f);

    public override IEnumerator Execute(FishController controller)
    {
        float durationTimer = 0f;// 这个计时器用来追踪整个行为的持续时间
        var rectTransform = controller.RectTransform;// 获取鱼的RectTransform组件，方便后续操作

        // 步骤1：初始化方向。
        // 如果鱼的当前位置在中心点(y=0)的下方，则第一次移动方向为上；反之为下。
        // 这确保了初始移动总是趋向于中心区域，表现更自然。
        bool isMovingUp = rectTransform.anchoredPosition.y < 0;

        // 步骤2：根据初始方向，计算出第一个目标点。
        float moveDistance = Random.Range(MinMaxJitterDistance.x, MinMaxJitterDistance.y);// 随机一个移动距离
        float targetY = rectTransform.anchoredPosition.y + (isMovingUp ? moveDistance : -moveDistance);// 根据方向决定是加还是减
        targetY = Mathf.Clamp(targetY, controller.FishMinYBoundary, controller.FishMaxYBoundary);// 确保目标点在鱼的活动范围内

        // --- 核心移动循环 ---
        while (durationTimer < Duration)
        {
            durationTimer += Time.deltaTime;// 累加总时长计时器

            // 步骤3：每帧都向当前的目标点平滑移动。
            Vector2 currentPos = rectTransform.anchoredPosition;// 当前鱼的位置
            Vector2 targetPos = new Vector2(currentPos.x, targetY);// 目标点的X坐标与当前相同，Y坐标为计算出的目标Y
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, controller.CurrentSpeed * Time.deltaTime);//计算出新的位置
            rectTransform.anchoredPosition = newPos;// 应用新的位置

            // 步骤4：【无停顿机制的核心】检查是否已“几乎”到达目标点。
            // 使用Mathf.Approximately可以避免浮点数精度问题。
            if (Mathf.Approximately(newPos.y, targetY))
            {
                // a. 到达后，立即反转下一次的移动方向
                isMovingUp = !isMovingUp;

                // b. 立即计算出一个位于新方向上的、新的目标点
                moveDistance = Random.Range(MinMaxJitterDistance.x, MinMaxJitterDistance.y);
                targetY = rectTransform.anchoredPosition.y + (isMovingUp ? moveDistance : -moveDistance);
                targetY = Mathf.Clamp(targetY, controller.FishMinYBoundary, controller.FishMaxYBoundary);
                
                // 协程会在此处结束当前帧，并在下一帧无缝地朝新目标点移动。
            }

            yield return null;
        }
    }
}
