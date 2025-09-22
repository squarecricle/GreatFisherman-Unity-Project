using UnityEngine;
using UnityEngine.UI; // 我们会用到UI组件，所以提前引入

public class CastingAndHookingController : MonoBehaviour//抛竿与提钩控制器
{
    // --- 公有变量 (Public Variables) ---
    // 我们预先把未来会用到的UI组件引用放在这里
    [Header("UI组件关联")]
    public GameObject CastingAndHookingPanel; // 整个玩法的UI容器
    public Slider PowerBar;                   // 蓄力条

    [Header("游戏参数")]
    public float PowerBarSpeed = 1f; // 蓄力条增长速度
    [Header("甜蜜点参数")]
    public RectTransform SweetSpotRect; // (注意！)把之前的Image SweetSpot改成这个
    public float SweetSpotMoveSpeed = 0.1f;
    public float SweetSpotWidth = 0.5f; // 甜蜜点的宽度 (0到1之间)

    // --- 私有变量 (Private Variables) ---
    // ... 已有变量 ...
    private bool _isPerfectCast; // 用于记录本次抛竿是否为“完美抛竿”

    // --- 私有变量 (Private Variables) ---
    // 使用枚举来定义所有可能的状态，清晰且不会出错
    private enum GameplayState
    {
        Inactive,       // 未激活
        Casting,        // 正在蓄力
        WaitingForBite, // 等待咬钩
        Hooking,        // 提线反应
        Success,        // 提钩成功
        Failed          // 提钩失败
    }
    private GameplayState _currentState; // 存储当前所处的状态

    // --- Unity生命周期函数 ---
    void Start()
    {
        // 初始状态下，整个UI是隐藏的
        CastingAndHookingPanel.SetActive(false);
        _currentState = GameplayState.Inactive;
        // 【临时测试代码】游戏一开始直接进入抛竿，方便我们看到效果
        // 等功能完成后可以删除这一行
        StartCastingProcess();
    }

    void Update()
    {
        // 这是状态机的核心：每一帧都根据当前状态，执行不同的逻辑
        switch (_currentState)
        {
            case GameplayState.Inactive:
                // 不做任何事
                break;
            case GameplayState.Casting:
                HandleCastingState();//在这里处理蓄力逻辑
                break;
            case GameplayState.WaitingForBite:
                // TODO: 在这里处理等待鱼上钩的计时
                break;
            case GameplayState.Hooking:
                // TODO: 在这里处理玩家的反应检测
                break;
            case GameplayState.Success:
                // TODO: 处理成功后的逻辑
                break;

            case GameplayState.Failed:
                // TODO: 处理失败后的逻辑
                break;
        }
    }

    // --- 公有方法 (Public Methods) ---
    // 这是提供给外部的入口，比如给一个“开始钓鱼”的按钮调用
    public void StartCastingProcess()//开始抛竿蓄力流程
    {
        CastingAndHookingPanel.SetActive(true);//显示钓鱼UI
        
        PowerBar.value = 0; // 每次开始蓄力时，都将进度条归零
        _isPerfectCast = false;//重置完美抛竿标记
        ChangeState(GameplayState.Casting);//切换到蓄力状态
    }

    // --- 私有方法 (Private Methods) ---
    // 一个专门用来改变状态的方法，让逻辑更集中
    private void ChangeState(GameplayState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        Debug.Log("状态切换为: " + _currentState); // 打印日志方便我们调试
    }
    private void HandleCastingState()
    {
    HandleSweetSpotMovement(); //处理甜蜜点的往复移动 (已修正)

    // 玩家按住鼠标时，持续增加力度条的值
    if (Input.GetMouseButton(0))
    {
        // 这里的蓄力逻辑我们后续可以用 AnimationCurve 替换
        PowerBar.value += PowerBarSpeed * (1 - PowerBar.value) * Time.deltaTime;
    }

    // 玩家一旦松开鼠标，就切换到下一个状态
    if (Input.GetMouseButtonUp(0))
    {
        // ---【逻辑修正区域】---
        // 1. 直接从 RectTransform 的锚点获取归一化的边界
        float sweetSpotMin = SweetSpotRect.anchorMin.x;
        float sweetSpotMax = SweetSpotRect.anchorMax.x;

        // 2. 判断力度条的值是否在边界内
        if (PowerBar.value >= sweetSpotMin && PowerBar.value <= sweetSpotMax)
        {
            _isPerfectCast = true;
            Debug.Log("完美抛竿 (Perfect Cast)!");
        }
        else
        {
            _isPerfectCast = false;
            Debug.Log("普通抛竿 (Normal Cast)");
        }
        
        // 3. 切换到等待咬钩状态
        ChangeState(GameplayState.WaitingForBite);
    }
    }
    /// <summary>
/// 处理甜蜜点的往复移动 (修正版)
/// </summary>
private void HandleSweetSpotMovement()
{
    // 1. 使用PingPong函数计算出甜蜜点“左边界”的归一化位置 (值在 0 和 1-SweetSpotWidth 之间)
    // 这确保了甜蜜点的右边界不会超出进度条的100%范围
    float leftEdgePosition = Mathf.PingPong(Time.time * SweetSpotMoveSpeed, 1 - SweetSpotWidth);

    // 2. 直接根据计算出的左边界位置，来更新左右两个锚点的x坐标
    SweetSpotRect.anchorMin = new Vector2(leftEdgePosition, SweetSpotRect.anchorMin.y);
    SweetSpotRect.anchorMax = new Vector2(leftEdgePosition + SweetSpotWidth, SweetSpotRect.anchorMax.y);

    // 3. [重要!] 当完全用anchors控制UI时，最好将offset归零，以避免任何意外偏移。
    SweetSpotRect.offsetMin = Vector2.zero;
    SweetSpotRect.offsetMax = Vector2.zero;
}
}