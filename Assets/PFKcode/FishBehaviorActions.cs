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

    [Tooltip("每次抖动的最大幅度（上下随机移动的距离）")]
    public float Magnitude = 50f;

    [Tooltip("两次抖动之间的间隔时间（秒），值越小抖动越频繁")]
    public float Interval = 0.1f;

    public override IEnumerator Execute(FishController controller)
    {
        float durationTimer = 0f;
        float intervalTimer = 0f;

        while (durationTimer < Duration)
        {
            durationTimer += Time.deltaTime;
            intervalTimer += Time.deltaTime;

            if (intervalTimer >= Interval)
            {
                float yOffset = Random.Range(-Magnitude, Magnitude);
                float targetY = controller.RectTransform.anchoredPosition.y + yOffset;
                targetY = Mathf.Clamp(targetY, controller.MinY, controller.MaxY);
                controller.RectTransform.anchoredPosition = new Vector2(controller.RectTransform.anchoredPosition.x, targetY);
                intervalTimer = 0f;
            }

            yield return null;
        }
    }
}