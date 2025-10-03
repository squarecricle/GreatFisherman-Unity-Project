using UnityEngine;
using UnityEngine.UI; // 我们会用到UI组件，所以提前引入

public class CastingAndHookingController : MonoBehaviour
{
    #region 公有变量
    [Header("UI组件关联")]
        public GameObject CastingAndHookingPanel; // 整个玩法的UI容器
    [Header("Hooking 阶段UI")]
        public Image HookIcon; // 拖入你场景中那个巨大的“感叹号”Image
    [Header("系统关联")]
        public FishingMiniGameManager FishingGameManager; // 拖入场景中的 FishMiniGameManager 对象
        public FishingSpot CurrentFishingSpot; // 拖入场景中的 FishingSpot 对象

    [Header("甜蜜点参数")]
        [Tooltip("完美抛竿时，提线反应的时间窗口(秒)")] 
        public RectTransform SweetSpotRect;
        [Tooltip("甜蜜点左右移动的速度")]
        public float SweetSpotMoveSpeed = 0.5f;
        [Tooltip("甜蜜点的宽度，以进度条总宽度的百分比表示 (0到1之间)")]
        [Range(0.1f, 0.9f)]
        public float SweetSpotWidth = 0.25f;//
        public ParabolicPowerBarController PowerBarController; // 蓄力条控制器脚本
    [Header("等待咬钩时间范围")]
        [Tooltip("等待咬钩的最小和最大时间（秒）")]
        public Vector2 WaitDurationRange = new Vector2(1.5f, 4.0f);
    [Header("提线反应时间配置")]
        [Tooltip("普通抛竿时，提线反应的时间窗口(秒)")]
        public float NormalHookTime = 0.5f;
        [Tooltip("完美抛竿时，提线反应的时间窗口(秒)")]
        public float PerfectHookTime = 1.5f;
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
    private float _sweetSpotTimer; // 【新增】甜蜜点的专属计时器
    #endregion 私有变量

    void Start()
    {
        // 初始状态下，整个UI是隐藏的
        CastingAndHookingPanel.SetActive(false);
        if (HookIcon != null) 
        {
            HookIcon.gameObject.SetActive(false);
        }
        _currentState = GameplayState.Inactive;
        //力度条清零
        PowerBarController.ResetPowerBar();

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
        PowerBarController.ResetPowerBar();//重置力度条 
        CastingAndHookingPanel.SetActive(true);//显示钓鱼UI
        ChangeState(GameplayState.ReadyToCast);//切换到准备抛竿状态
        if (SweetSpotRect != null)//确保开始时甜蜜点是隐藏的
        {
            SweetSpotRect.gameObject.SetActive(false);
        }    
    }

    #endregion
    #region 私有方法
    private void ChangeState(GameplayState newState)//切换状态
    {
        if (_currentState == newState) return;// 如果状态没有变化，直接返回

        _currentState = newState;// 更新当前状态
        Debug.Log("状态切换为: " + _currentState); // 打印日志方便我们调试

        // --- 状态切换时的“一次性”逻辑 ---
        if (_currentState == GameplayState.WaitingForBite)
        {
            // 当进入“等待咬钩”状态时，启动等待协程
            StartCoroutine(WaitingForBiteCoroutine());
        }
        else if (_currentState == GameplayState.Hooking)
        {
            StartCoroutine(HookingCoroutine());
        }
    }

    private void HandleReadyToCastState()
    {
        // 在准备状态下，我们只等待玩家按下鼠标                
        if (Input.GetMouseButtonDown(0))
        {
            if (SweetSpotRect != null)
            {
                SweetSpotRect.gameObject.SetActive(true);
            }
            _sweetSpotTimer = 0f;
            // 捕获到按下的瞬间，立即命令工具开始蓄力 
            HandleSweetSpotMovement(); // 确保甜蜜点位置初始化正确           
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
            // 3. 隐藏甜蜜点
            if (SweetSpotRect != null)
            {
                SweetSpotRect.gameObject.SetActive(false);
            }
            // 3. 切换到等待咬钩状态
            ChangeState(GameplayState.WaitingForBite);
        }
    }
    private void HandleSweetSpotMovement()
    {
        _sweetSpotTimer += Time.deltaTime;//使用专属计时器

        // 1. 使用PingPong函数计算出甜蜜点“左边界”的归一化位置 (值在 0 和 1-SweetSpotWidth 之间)
        //    这确保了甜蜜点的右边界不会超出进度条的100%范围
        float leftEdgePosition = Mathf.PingPong(_sweetSpotTimer * SweetSpotMoveSpeed, 1 - SweetSpotWidth);  
        // 2. 直接根据计算出的左边界位置，来更新左右两个锚点的x坐标
        SweetSpotRect.anchorMin = new Vector2(leftEdgePosition, SweetSpotRect.anchorMin.y);
        SweetSpotRect.anchorMax = new Vector2(leftEdgePosition + SweetSpotWidth, SweetSpotRect.anchorMax.y);

        // 3. [重要!] 当完全用anchors控制UI时，最好将offset归零，以避免任何意外偏移。
        SweetSpotRect.offsetMin = Vector2.zero;
        SweetSpotRect.offsetMax = Vector2.zero;
    }
    private System.Collections.IEnumerator WaitingForBiteCoroutine()//等待咬钩协程
    {
        // 1. 在我们设定的范围内，随机一个等待时间
        float waitTime = Random.Range(WaitDurationRange.x, WaitDurationRange.y);
        Debug.Log($"鱼将在 {waitTime:F2} 秒后咬钩...");

        // 2. 使用 yield return 等待指定的时间
        yield return new WaitForSeconds(waitTime);

        // 3. 时间到，切换到提线反应状态
        ChangeState(GameplayState.Hooking);
    }
        private System.Collections.IEnumerator HookingCoroutine()//提线反应协程
    {
        // --- 1. 初始化 ---
        // 根据是否“完美抛竿”来决定总时长
        float duration = _isPerfectCast ? PerfectHookTime : NormalHookTime;
        Debug.Log($"提线窗口: {duration:F2} 秒，完美抛竿: {_isPerfectCast}");

        // 激活感叹号，并重置其状态
        HookIcon.gameObject.SetActive(true);
        CanvasGroup hookCanvasGroup = HookIcon.GetComponent<CanvasGroup>();
        hookCanvasGroup.alpha = 1f; // 完全不透明
        HookIcon.rectTransform.localScale = Vector3.one; // 原始大小

        // --- 2. 核心循环：等待玩家输入或计时结束 ---
        float elapsedTime = 0f;
        bool caughtInTime = false;

        while (elapsedTime < duration)
        {
            // 检查玩家是否点击
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("玩家在规定时间内点击！");
                caughtInTime = true;
                break; // 玩家已点击，立即跳出循环
            }

            // --- 3. 动画处理 ---
            // 计算当前进度 (从0到1)
            float progress = elapsedTime / duration;

            // 根据进度，平滑地修改透明度和大小
            // Alpha 从 1 (不透明) -> 0 (透明)
            hookCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            // Scale 从 1 (原始大小) -> 0.1 (很小)
            HookIcon.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.1f, progress);

            // 时间累加，并等待下一帧
            elapsedTime += Time.deltaTime;
            yield return null; // yield return null 表示“等待到下一帧再继续执行”
        }

        // --- 4. 结果处理 ---
        // 无论循环是因为时间到还是因为被break，都先隐藏感叹号
        HookIcon.gameObject.SetActive(false);

        if (caughtInTime)
        {
            Debug.Log("提线成功！启动“与鱼博弈”小游戏！");
            FishingGameManager.TriggerMiniGameStartSequence(CurrentFishingSpot); // 不再直接启动小游戏，而是请求播放过场动画
            CastingAndHookingPanel.SetActive(false);//隐藏钓鱼UI
            ChangeState(GameplayState.Success);//切换到成功状态
        }
        else
        {
            Debug.Log("太慢了，鱼跑掉了！");// 提线失败
            StartCastingProcess(); // 重新开始抛竿流程
        }

    }
    #endregion 私有方法
}