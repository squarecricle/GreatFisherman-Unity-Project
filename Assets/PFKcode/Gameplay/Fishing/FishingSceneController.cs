using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 钓鱼面板的总控制器，负责协调钓点选择、抛竿模块与迷你游戏之间的流程。
/// </summary>
public class FishingSceneController : MonoBehaviour
{
    [Header("核心 UI 组件")]
    [SerializeField] private TextMeshProUGUI spotTitleText;
    [SerializeField] private GameObject fishingUIPanel;

    [Header("功能控制器")]
    [SerializeField] private CastingAndHookingController castingController;
    [SerializeField] private FishingMiniGameManager miniGameManager;

    [Header("默认钓点列表")]
    [SerializeField] private FishingSpot creekSpot;
    [SerializeField] private FishingSpot forestLakeSpot;
    [SerializeField] private FishingSpot beachSpot;

    private FishingSpot _activeSpot;
    private CatchableData _pendingCatchable;

    private readonly WaitForEndOfFrame _waitForNextFrame = new();

    private void Awake()
    {
        if (miniGameManager != null)
        {
            miniGameManager.SceneController = this;
        }
    }

    public void SelectCreekSpot()
    {
        RequestFishing(creekSpot, "小溪");
    }

    public void SelectForestLakeSpot()
    {
        RequestFishing(forestLakeSpot, "森林湖");
    }

    public void SelectBeachSpot()
    {
        RequestFishing(beachSpot, "沙滩");
    }

    private void RequestFishing(FishingSpot spot, string displayName)
    {
        if (spot == null)
        {
            Debug.LogError($"[FishingSceneController] 钓点 {displayName} 未在 Inspector 中设置。");
            return;
        }

        GameManager.Instance?.GoToFishing(spot, displayName);
    }

    /// <summary>
    /// 由 GameManager 调用以准备一次钓鱼流程。
    /// </summary>
    public void PrepareFishingSession(FishingSpot spot, string displayName)
    {
        if (miniGameManager == null || castingController == null)
        {
            Debug.LogError("[FishingSceneController] 缺少核心控制器引用，无法启动钓鱼流程。");
            return;
        }

        _activeSpot = spot;
        castingController.CurrentFishingSpot = spot;
        _pendingCatchable = spot.SelectItemByWeight();

        if (_pendingCatchable == null)
        {
            Debug.LogError("[FishingSceneController] 钓点未返回有效渔获，流程中止。");
            return;
        }

        miniGameManager.CurrentCatchableData = _pendingCatchable;
        if (spotTitleText != null)
        {
            spotTitleText.text = displayName;
        }

        if (fishingUIPanel != null)
        {
            fishingUIPanel.SetActive(true);
        }

        StartCoroutine(DelayedStartCasting());
    }

    public void RestartFishingProcess()
    {
        if (_activeSpot == null)
        {
            Debug.LogWarning("[FishingSceneController] 没有激活的钓点，忽略重开请求。");
            return;
        }

        if (miniGameManager == null || castingController == null)
        {
            Debug.LogError("[FishingSceneController] 缺少核心控制器引用，无法重新开始钓鱼。");
            return;
        }

        _pendingCatchable = _activeSpot.SelectItemByWeight();
        if (_pendingCatchable == null)
        {
            Debug.LogError("[FishingSceneController] 钓点未返回有效渔获，无法重新开始。");
            GameManager.Instance?.GoToHome();
            return;
        }

        miniGameManager.CurrentCatchableData = _pendingCatchable;
        castingController.StartCastingProcess();
    }

    public void HandleReturnToMenu()
    {
        if (fishingUIPanel != null)
        {
            fishingUIPanel.SetActive(false);
        }
        GameManager.Instance?.GoToHome();
    }

    public void NotifyFishingSessionEnded()
    {
        _activeSpot?.OnFishingSessionEnd();
    }

    private IEnumerator DelayedStartCasting()
    {
        yield return _waitForNextFrame;
        castingController.StartCastingProcess();
    }
}
