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
            // 1. 先“提名”出下一帧应该播放哪个序列
            List<FishAction> nextSequence = (_gameManager.ProgressBar.value >= _currentFishData.StruggleThreshold)
                ? _currentFishData.StruggleBehaviorSequence
                : _currentFishData.CalmBehaviorSequence;

            // 2. 检查“提名”的序列和“当前正在播放”的序列是不是同一个
            if (nextSequence != _currentBehaviorSequence)
            {
                // 如果不是同一个，说明鱼的状态发生了切换（冷静 <-> 挣扎）
                // 此时必须重置索引，否则就会发生你遇到的bug！
                _currentBehaviorSequence = nextSequence;
                _currentActionIndex = 0;
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
                yield return new WaitForSeconds(waitAction.Duration);// 直接等待指定时间
            }
            else if (currentAction is Jump_Action jumpAction)
            {
                float pauseTime = Random.Range(jumpAction.PauseDurationRange.x, jumpAction.PauseDurationRange.y);// 随机一个停顿时间
                
                yield return new WaitForSeconds(pauseTime);// 等待停顿时间

                float targetY = Random.Range(_minY, _maxY);// 随机一个新的Y位置
                _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, targetY);// 瞬移到新位置
            }
            else if (currentAction is ChangeSpeed_Action changeSpeedAction)
            {
                _currentSpeed = changeSpeedAction.NewSpeed;// 直接改变当前速度
                yield return null; // 改变速度是瞬间的，等待一帧继续
            }
            else if (currentAction is Jitter_Action jitterAction)
            {
                // 计时器，用于控制整个抖动行为的总时长
                _actionTimer = 0f; 
                // 独立计时器，用于控制两次抖动之间的间隔
                float jitterTimer = 0f; 

                // 在指定的总时长内，持续执行抖动逻辑
                while (_actionTimer < jitterAction.Duration)
                {
                    // 两个计时器同时累加
                    _actionTimer += Time.deltaTime;
                    jitterTimer += Time.deltaTime;

                    // 当间隔计时器到达指定间隔时
                    if (jitterTimer >= jitterAction.Interval)
                    {
                        // 1. 计算一个随机的Y轴偏移量
                        float yOffset = Random.Range(-jitterAction.Magnitude, jitterAction.Magnitude);
                        
                        // 2. 在当前位置的基础上应用偏移，得到目标位置
                        float targetY = _rectTransform.anchoredPosition.y + yOffset;

                        // 3. 【安全措施】确保目标位置不会超出活动边界
                        targetY = Mathf.Clamp(targetY, _minY, _maxY);

                        // 4. 瞬间移动到新的目标位置
                        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, targetY);

                        // 5. 重置间隔计时器，准备下一次抖动
                        jitterTimer = 0f;
                    }
                    
                    // 等待下一帧，让游戏继续进行
                    yield return null; 
                }
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