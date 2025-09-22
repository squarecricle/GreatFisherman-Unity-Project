using UnityEngine;
using UnityEngine.UI; // 我们会用到UI组件，所以提前引入

public class CastingAndHookingController : MonoBehaviour
{
    #region 公有变量
    [Header("UI组件关联")]
    public GameObject CastingAndHookingPanel; // 整个玩法的UI容器
    [Header("甜蜜点参数")]
    [Tooltip("拖入你的 SweetSpot Image 对象")]
    public RectTransform SweetSpotRect;
    [Tooltip("甜蜜点左右移动的速度")]
    public float SweetSpotMoveSpeed = 0.5f;
    [Tooltip("甜蜜点的宽度，以进度条总宽度的百分比表示 (0到1之间)")]
    [Range(0.1f, 0.9f)]
    public float SweetSpotWidth = 0.25f;
    public ParabolicPowerBarController PowerBarController; // 蓄力条控制器脚本
    #endregion 公有变量
    #region 私有变量
    private enum GameplayState
    {
        Inactive,       // 未激活
        ReadyToCast,   // 准备抛竿
        Casting,        // 正在蓄力
        WaitingForBite, // 等待咬钩
        Hooking,        // 提线反应
        Success,        // 提钩成功
        Failed          // 提钩失败
    }
    private GameplayState _currentState; // 存储当前所处的状态
    private bool _isPerfectCast; // 用于记录本次抛竿是否为“完美抛竿”

    #endregion 私有变量

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
            case GameplayState.ReadyToCast:
                HandleReadyToCastState();//在这里处理准备抛竿逻辑
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

    #region 公有方法
    public void StartCastingProcess()//开始蓄力流程
    {
        CastingAndHookingPanel.SetActive(true);//显示钓鱼UI
        ChangeState(GameplayState.ReadyToCast);//切换到准备抛竿状态
        // 直接切换到抛竿状态，方便测试
    }

    #endregion
    #region 私有方法
    private void ChangeState(GameplayState newState)//切换状态
    {
        if (_currentState == newState) return;// 如果状态没有变化，直接返回

        _currentState = newState;// 更新当前状态
        Debug.Log("状态切换为: " + _currentState); // 打印日志方便我们调试
    }

    private void HandleReadyToCastState()
    {
        // 在准备状态下，我们只等待玩家按下鼠标                
        if (Input.GetMouseButtonDown(0))
        {
            // 捕获到按下的瞬间，立即命令工具开始蓄力            
            PowerBarController.StartCharging();
            // 然后立刻切换到“正在蓄力”状态                      
            ChangeState(GameplayState.Casting);
        }
    }
private void HandleCastingState()//处理蓄力状态
{
    // 在蓄力状态下，每一帧都驱动甜蜜点移动
    HandleSweetSpotMovement();

    // 当玩家“松开”鼠标左键的瞬间
    if (Input.GetMouseButtonUp(0))
    {
        // 命令“专业工具”停止蓄力
        PowerBarController.StopCharging(); 
        // 从“专业工具”那里获取最终的蓄力结果
        float finalPowerValue = PowerBarController.PowerBarSlider.value;
        Debug.Log("获取到最终蓄力值: " + finalPowerValue);

        // ---【甜蜜点判定逻辑】---
        // 1. 直接从 RectTransform 的锚点获取归一化的边界
        float sweetSpotMin = SweetSpotRect.anchorMin.x;
        float sweetSpotMax = SweetSpotRect.anchorMax.x;

        // 2. 判断力度条的值是否在边界内
        if (finalPowerValue >= sweetSpotMin && finalPowerValue <= sweetSpotMax)
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
    private void HandleSweetSpotMovement()
    {
        // 1. 使用PingPong函数计算出甜蜜点“左边界”的归一化位置 (值在 0 和 1-SweetSpotWidth 之间)
        //    这确保了甜蜜点的右边界不会超出进度条的100%范围
        float leftEdgePosition = Mathf.PingPong(Time.time * SweetSpotMoveSpeed, 1 - SweetSpotWidth);

        // 2. 直接根据计算出的左边界位置，来更新左右两个锚点的x坐标
        SweetSpotRect.anchorMin = new Vector2(leftEdgePosition, SweetSpotRect.anchorMin.y);
        SweetSpotRect.anchorMax = new Vector2(leftEdgePosition + SweetSpotWidth, SweetSpotRect.anchorMax.y);

        // 3. [重要!] 当完全用anchors控制UI时，最好将offset归零，以避免任何意外偏移。
        SweetSpotRect.offsetMin = Vector2.zero;
        SweetSpotRect.offsetMax = Vector2.zero;
    }
    #endregion 私有方法
}