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
    public float SweetSpotWidth = 0.25f;
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
        ReadyToCast,    // 准备抛竿
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

    private void Start()
    {
        CastingAndHookingPanel.SetActive(false);
        if (HookIcon != null)
        {
            HookIcon.gameObject.SetActive(false);
        }

        _currentState = GameplayState.Inactive;
        PowerBarController.ResetPowerBar();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case GameplayState.Inactive:
                break;
            case GameplayState.ReadyToCast:
                HandleReadyToCastState();
                break;
            case GameplayState.Casting:
                HandleCastingState();
                break;
            case GameplayState.WaitingForBite:
                break;
            case GameplayState.Hooking:
                break;
            case GameplayState.Success:
                break;
            case GameplayState.Failed:
                break;
        }
    }

    #region 公有方法
    public void StartCastingProcess()
    {
        PowerBarController.ResetPowerBar();
        CastingAndHookingPanel.SetActive(true);
        ChangeState(GameplayState.ReadyToCast);
        if (SweetSpotRect != null)
        {
            SweetSpotRect.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 私有方法
    private void ChangeState(GameplayState newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        _currentState = newState;
        Debug.Log("状态切换为: " + _currentState);

        if (_currentState == GameplayState.WaitingForBite)
        {
            StartCoroutine(WaitingForBiteCoroutine());
        }
        else if (_currentState == GameplayState.Hooking)
        {
            StartCoroutine(HookingCoroutine());
        }
    }

    private void HandleReadyToCastState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (SweetSpotRect != null)
            {
                SweetSpotRect.gameObject.SetActive(true);
            }

            _sweetSpotTimer = 0f;
            HandleSweetSpotMovement();
            PowerBarController.StartCharging();
            ChangeState(GameplayState.Casting);
        }
    }

    private void HandleCastingState()
    {
        HandleSweetSpotMovement();

        if (Input.GetMouseButtonUp(0))
        {
            PowerBarController.StopCharging();
            float finalPowerValue = PowerBarController.PowerBarSlider.value;
            Debug.Log("获取到最终蓄力值: " + finalPowerValue);

            float sweetSpotMin = SweetSpotRect.anchorMin.x;
            float sweetSpotMax = SweetSpotRect.anchorMax.x;

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

            if (SweetSpotRect != null)
            {
                SweetSpotRect.gameObject.SetActive(false);
            }

            ChangeState(GameplayState.WaitingForBite);
        }
    }

    private void HandleSweetSpotMovement()
    {
        _sweetSpotTimer += Time.deltaTime;

        float leftEdgePosition = Mathf.PingPong(_sweetSpotTimer * SweetSpotMoveSpeed, 1 - SweetSpotWidth);
        SweetSpotRect.anchorMin = new Vector2(leftEdgePosition, SweetSpotRect.anchorMin.y);
        SweetSpotRect.anchorMax = new Vector2(leftEdgePosition + SweetSpotWidth, SweetSpotRect.anchorMax.y);
        SweetSpotRect.offsetMin = Vector2.zero;
        SweetSpotRect.offsetMax = Vector2.zero;
    }

    private System.Collections.IEnumerator WaitingForBiteCoroutine()
    {
        float waitTime = Random.Range(WaitDurationRange.x, WaitDurationRange.y);
        Debug.Log($"鱼将在 {waitTime:F2} 秒后咬钩...");

        yield return new WaitForSeconds(waitTime);

        ChangeState(GameplayState.Hooking);
    }

    private System.Collections.IEnumerator HookingCoroutine()
    {
        float duration = _isPerfectCast ? PerfectHookTime : NormalHookTime;
        Debug.Log($"提线窗口: {duration:F2} 秒，完美抛竿: {_isPerfectCast}");

        HookIcon.gameObject.SetActive(true);
        CanvasGroup hookCanvasGroup = HookIcon.GetComponent<CanvasGroup>();
        hookCanvasGroup.alpha = 1f;
        HookIcon.rectTransform.localScale = Vector3.one;

        float elapsedTime = 0f;
        bool caughtInTime = false;

        while (elapsedTime < duration)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("玩家在规定时间内点击！");
                caughtInTime = true;
                break;
            }

            float progress = elapsedTime / duration;
            hookCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            HookIcon.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.1f, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        HookIcon.gameObject.SetActive(false);

        if (caughtInTime)
        {
            Debug.Log("提线成功！启动“与鱼博弈”小游戏！");
            FishingGameManager.TriggerMiniGameStartSequence(CurrentFishingSpot);
            CastingAndHookingPanel.SetActive(false);
            ChangeState(GameplayState.Success);
        }
        else
        {
            Debug.Log("太慢了，鱼跑掉了！");
            StartCastingProcess();
        }
    }
    #endregion 私有方法
}
