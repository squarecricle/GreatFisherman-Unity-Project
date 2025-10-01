using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 控制“鱼”在迷你游戏中的所有行为，包括移动、状态切换和执行行为序列。
/// </summary>
public class FishController : MonoBehaviour
{
    // --- 核心数据 ---
    private FishData _currentFishData;              // 当前正在钓的鱼的配置数据。
    private List<FishAction> _currentBehaviorSequence; // 当前正在执行的行为序列（冷静或挣扎）。

    // --- 组件引用 ---
    private RectTransform _rectTransform;           // 鱼自身UI元素的RectTransform组件。
    private FishingMiniGameManager _gameManager;    // 游戏总管的引用，用于获取游戏状态（如进度条）。

    // --- 状态变量 ---
    private float _currentSpeed;                    // 鱼当前的移动速度，可以被ChangeSpeed_Action动态修改。
    private int _currentActionIndex = 0;            // 当前执行的行为在序列中的索引。
    private Coroutine _behaviorCoroutine;           // 用于控制和停止鱼行为的协程。

    // --- 边界与坐标 ---
    private float _fishMinYBoundary;                // 鱼可以移动到的最小Y坐标（考虑了自身高度）。
    private float _fishMaxYBoundary;                // 鱼可以移动到的最大Y坐标（考虑了自身高度）。
    
    /// <summary> 鱼图标顶部的当前Y坐标 </summary>
    public float TopY => _rectTransform.anchoredPosition.y + _rectTransform.rect.height / 2;
    /// <summary> 鱼图标底部的当前Y坐标 </summary>
    public float BottomY => _rectTransform.anchoredPosition.y - _rectTransform.rect.height / 2;

    #region 公开给Action访问的属性
    // 这些属性为FishAction提供了执行时所需的信息和控制权，是重构后的重要接口。
    public RectTransform RectTransform => _rectTransform;
    public float FishMinYBoundary => _fishMinYBoundary;
    public float FishMaxYBoundary => _fishMaxYBoundary;
    public float CurrentSpeed 
    {
        get => _currentSpeed;
        set => _currentSpeed = value;
    }
    #endregion

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 由GameManager在游戏开始时调用，用于初始化鱼的所有状态。
    /// </summary>
    /// <param name="data">要使用的鱼的数据</param>
    /// <param name="fishingAreaHeight">钓鱼区域的总高度</param>
    /// <param name="manager">游戏总管的实例</param>
    public void Initialize(FishData data, float fishingAreaHeight, FishingMiniGameManager manager)
    {
        _currentFishData = data;
        _gameManager = manager;

        // --- 计算鱼的活动边界 ---
        // 钓鱼区域的中心点Y坐标为0。其顶部Y坐标为 fishingAreaHeight/2，底部为 -fishingAreaHeight/2。
        // 为了防止鱼图标的身体“出界”，我们需要将边界向内缩进“半个鱼的高度”。
        float halfFishHeight = _rectTransform.rect.height / 2;
        _fishMinYBoundary = -fishingAreaHeight / 2 + halfFishHeight;
        _fishMaxYBoundary = fishingAreaHeight / 2 - halfFishHeight;

        // --- 重置状态 ---
        _currentSpeed = _currentFishData.BaseMoveSpeed;
        _currentActionIndex = 0;
        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, 0); // 将鱼重置到中心位置
    }

    /// <summary>
    /// 开始执行鱼的行为逻辑协程。
    /// </summary>
    public void StartBehavior()
    {
        StopBehavior(); // 安全起见，先停止所有可能正在运行的旧协程。
        this.enabled = true; // 激活组件，让Update可以运行（如果未来需要的话）。
        _behaviorCoroutine = StartCoroutine(FishBehavior());
    }

    /// <summary>
    /// 停止鱼的行为逻辑协程。
    /// </summary>
    public void StopBehavior()
    {
        if (_behaviorCoroutine != null)
        {
            StopCoroutine(_behaviorCoroutine);
            _behaviorCoroutine = null;
        }
        this.enabled = false; // 禁用组件，可以停止Update的执行，节省性能。
    }

    /// <summary>
    /// 鱼行为的核心协程，一个无限循环，负责驱动整个行为逻辑。
    /// </summary>
    private IEnumerator FishBehavior()
    {
        while (true)
        {
            // 1. 决定行为序列：根据进度条是否超过阈值，决定鱼是“冷静”还是“挣扎”。
            List<FishAction> nextSequence = (_gameManager.ProgressBar.value >= _currentFishData.StruggleThreshold)
                ? _currentFishData.StruggleBehaviorSequence
                : _currentFishData.CalmBehaviorSequence;
            
            // 2. 处理状态切换：如果行为序列发生了变化，重置行为索引。
            if (nextSequence != _currentBehaviorSequence)
            {
                _currentBehaviorSequence = nextSequence;
                _currentActionIndex = 0;
            }

            // 3. 安全检查：如果序列为空或无内容，则等待一帧，避免报错。
            if (_currentBehaviorSequence == null || _currentBehaviorSequence.Count == 0)
            {
                yield return null;
                continue;
            }

            // 4. 执行行为：获取并执行当前的行为Action，并等待它完成。
            FishAction currentAction = _currentBehaviorSequence[_currentActionIndex];
            yield return StartCoroutine(currentAction.Execute(this));

            // 5. 推进索引：移动到序列中的下一个行为，如果到了末尾则从头开始。
            _currentActionIndex++;
            if (_currentActionIndex >= _currentBehaviorSequence.Count)
            {
                _currentActionIndex = 0;
            }
        }
    }
}