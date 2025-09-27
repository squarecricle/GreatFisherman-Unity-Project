using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishController : MonoBehaviour
{
    [Header("状态变量")]
    private FishData _currentFishData;    // 当前上钩鱼的行为数据
    private RectTransform _rectTransform; // 自身的RectTransform组件
    private float _fishTargetY;           // 鱼的目标Y坐标
    private float _minY;                  // 活动范围的最小Y值
    private float _maxY;                  // 活动范围的最大Y值
    private Coroutine _behaviorCoroutine; // 用于存储和控制行为协程
    public float TopY => _rectTransform.anchoredPosition.y + _rectTransform.rect.height / 2;
    public float BottomY => _rectTransform.anchoredPosition.y - _rectTransform.rect.height / 2;
    private float _currentSpeed;                  // 鱼当前的基础移动速度
    private int _currentActionIndex = 0;          // 当前执行的行为在序列中的索引
    private float _actionTimer = 0f;              // 当前行为的计时器
    private List<FishAction> _currentBehaviorSequence; // 当前正在执行的行为序列（冷静或挣扎）
    private FishingMiniGameManager _gameManager;
    void Awake()
    {
        // 提前获取组件引用，这是一个好习惯
        _rectTransform = GetComponent<RectTransform>();
    }

    // 由GameManager调用的初始化方法
    public void Initialize(FishData data, float fishingAreaHeight, FishingMiniGameManager manager)
    {
        _currentFishData = data;
        _gameManager = manager; // 获取总管的引用

        // 根据钓鱼区域高度，计算自己的活动边界
        float halfFishHeight = _rectTransform.rect.height / 2;
        _minY = -fishingAreaHeight / 2 + halfFishHeight;
        _maxY = fishingAreaHeight / 2 - halfFishHeight;

        // --- 重置新系统的状态 ---
        _currentSpeed = _currentFishData.BaseMoveSpeed; // 设置初始速度
        _currentActionIndex = 0; // 从第一个行为开始
        _actionTimer = 0f;
        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, 0);
    }

    // 在自己的Update中处理移动，只有当组件被激活时才会执行
    void Update()
    {

    }


    // 开始执行鱼的行为逻辑
    public void StartBehavior()
    {
        // 启动前，确保停止所有旧的协程，并将组件激活
        StopBehavior();
        this.enabled = true;
        _behaviorCoroutine = StartCoroutine(FishBehavior());
    }

    // 停止鱼的行为逻辑
    public void StopBehavior()
    {
        if (_behaviorCoroutine != null)
        {
            StopCoroutine(_behaviorCoroutine);
            _behaviorCoroutine = null;
        }
        // 禁用组件可以停止Update的执行，节省性能
        this.enabled = false;
    }

    private IEnumerator FishBehavior()
    {
        // 只要这个协程在运行，就不断地执行行为序列
        while (true)
        {
            // 1. 根据进度条决定使用哪个行为序列
            if (_gameManager.ProgressBar.value >= _currentFishData.StruggleThreshold)
            {
                _currentBehaviorSequence = _currentFishData.StruggleBehaviorSequence;
            }
            else
            {
                _currentBehaviorSequence = _currentFishData.CalmBehaviorSequence;
            }

            // 安全检查：如果序列为空，就等待一帧避免报错
            if (_currentBehaviorSequence == null || _currentBehaviorSequence.Count == 0)
            {
                yield return null;
                continue; // 跳过本次循环，进入下一帧
            }

            // 2. 获取当前要执行的行为
            FishAction currentAction = _currentBehaviorSequence[_currentActionIndex];

            // 3. 【核心】根据行为的“类型”，执行不同的逻辑
            if (currentAction is Move_Action moveAction)
            {
                _actionTimer = 0f;
                float targetY = Random.Range(_minY, _maxY); // 随机一个目标点
                Vector2 targetPos = new Vector2(_rectTransform.anchoredPosition.x, targetY);

                // 在指定时间内，持续向目标移动
                while (_actionTimer < moveAction.Duration)
                {
                    _rectTransform.anchoredPosition = Vector2.MoveTowards(
                        _rectTransform.anchoredPosition,
                        targetPos,
                        _currentSpeed * Time.deltaTime); // 使用当前速度

                    _actionTimer += Time.deltaTime;
                    yield return null; // 等待下一帧
                }
            }
            else if (currentAction is Wait_Action waitAction)
            {
                yield return new WaitForSeconds(waitAction.Duration);
            }
            else if (currentAction is Jump_Action jumpAction)
            {
                float pauseTime = Random.Range(jumpAction.PauseDurationRange.x, jumpAction.PauseDurationRange.y);
                yield return new WaitForSeconds(pauseTime);

                float targetY = Random.Range(_minY, _maxY);
                _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, targetY);
            }
            else if (currentAction is ChangeSpeed_Action changeSpeedAction)
            {
                _currentSpeed = changeSpeedAction.NewSpeed;
                yield return null; // 改变速度是瞬间的，等待一帧继续
            }

            // 4. 移动到序列中的下一个行为
            _currentActionIndex++;
            if (_currentActionIndex >= _currentBehaviorSequence.Count)
            {
                _currentActionIndex = 0; // 如果到了末尾，从头循环
            }
        }
    }
}