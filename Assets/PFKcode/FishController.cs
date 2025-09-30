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

    private List<FishAction> _currentBehaviorSequence; // 当前正在执行的行为序列（冷静或挣扎）
    private FishingMiniGameManager _gameManager;
    // --- 公开属性，用于给 FishAction 提供执行时所需的信息和控制权 ---
    public RectTransform RectTransform => _rectTransform;
    public float MinY => _minY;
    public float MaxY => _maxY;
    public float CurrentSpeed 
    {
        get => _currentSpeed;
        set => _currentSpeed = value;
    }   
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
        // 根据当前进度条值，选择合适的行为序列
        List<FishAction> nextSequence = (_gameManager.ProgressBar.value >= _currentFishData.StruggleThreshold)
            ? _currentFishData.StruggleBehaviorSequence
            : _currentFishData.CalmBehaviorSequence;
        //
        if (nextSequence != _currentBehaviorSequence)
        {
            _currentBehaviorSequence = nextSequence;
            _currentActionIndex = 0;
        }
        // ---【修正结束】---

        // 安全检查
        if (_currentBehaviorSequence == null || _currentBehaviorSequence.Count == 0)
        {
            yield return null;//返回一帧，避免死循环
            continue;//跳过本次循环
        }

        // 【重构核心】
        // 1. 获取当前行为
        FishAction currentAction = _currentBehaviorSequence[_currentActionIndex];

        // 2. 直接命令这个行为“执行！”，然后等待它完成。
        //    我们不需要关心它具体是Move还是Wait，这就是面向对象“多态”的威力。
        yield return StartCoroutine(currentAction.Execute(this));//执行当前行为，并等待其完成，等待期间协程暂停

        // 3. 移动到下一个行为索引
        _currentActionIndex++;
        if (_currentActionIndex >= _currentBehaviorSequence.Count)
        {
            _currentActionIndex = 0;
        }   
        }
    }
}