using UnityEngine;
using System.Collections;

public class FishController : MonoBehaviour
{
    [Header("状态变量")]
    private FishData _currentFishData;    // 当前上钩鱼的行为数据
    private RectTransform _rectTransform; // 自身的RectTransform组件
    private float _fishTargetY;           // 鱼的目标Y坐标
    private float _minY;                  // 活动范围的最小Y值
    private float _maxY;                  // 活动范围的最大Y值
    private Coroutine _behaviorCoroutine; // 用于存储和控制行为协程

    void Awake()
    {
        // 提前获取组件引用，这是一个好习惯
        _rectTransform = GetComponent<RectTransform>();
    }

    // 由GameManager调用的初始化方法
    public void Initialize(FishData data, float fishingAreaHeight)
    {
        _currentFishData = data;

        // 根据钓鱼区域高度，计算自己的活动边界
        float halfFishHeight = _rectTransform.rect.height / 2;
        _minY = -fishingAreaHeight / 2 + halfFishHeight;
        _maxY = fishingAreaHeight / 2 - halfFishHeight;

        // 重置位置
        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, 0);
        _fishTargetY = 0; // 初始目标就是中间
    }

    // 在自己的Update中处理移动，只有当组件被激活时才会执行
    void Update()
    {
        MoveFish();
    }

    private void MoveFish()
    {
        if (_currentFishData == null) return; // 安全检查

        // 平滑地将鱼移动到目标位置
        Vector2 currentPos = _rectTransform.anchoredPosition;
        Vector2 targetPos = new Vector2(currentPos.x, _fishTargetY);
        _rectTransform.anchoredPosition = Vector2.MoveTowards(currentPos, targetPos, _currentFishData.MoveSpeed * Time.deltaTime);
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
        // 只要这个协程在运行，就不断为鱼设定新的目标
        while (true) 
        {
            switch (_currentFishData.FishBehavior)
            {
                case FishData.FishBehaviorType.平滑移动:
                    _fishTargetY = Random.Range(_minY, _maxY);
                    yield return new WaitForSeconds(Random.Range(_currentFishData.MinPauseDuration, _currentFishData.MaxPauseDuration));
                    break;
                
                default:
                    Debug.LogError($"[FishController] 遇到未处理的鱼行为类型: {_currentFishData.FishBehavior}");
                    yield return new WaitForSeconds(1f); // 安全保底行为
                    break;
            }
        }
    }
}