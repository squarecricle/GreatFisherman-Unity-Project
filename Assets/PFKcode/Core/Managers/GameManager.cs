using UnityEngine;

/// <summary>
/// 全局游戏状态机，根据架构文档负责跨系统流程的统一调度。
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Home,
        InFishing,
        ViewingFishTank,
        ViewingShop,
        ViewingMap
    }

    public static GameManager Instance { get; private set; }

    [Header("核心管理器依赖")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerDataManager playerDataManager;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("模块控制器")]
    [SerializeField] private FishingSceneController fishingSceneController;

    [Header("UI 面板名称映射")]
    [SerializeField] private string homePanelName = "Panel_Home";
    [SerializeField] private string fishingPanelName = "Panel_Fishing";
    [SerializeField] private string fishTankPanelName = "Panel_FishTank";
    [SerializeField] private string shopPanelName = "Panel_Shop";
    [SerializeField] private string mapPanelName = "Panel_Map";

    public GameState CurrentState { get; private set; }

    public PlayerDataManager PlayerData => playerDataManager;
    public InventoryManager Inventory => inventoryManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResolveDependencies();
    }

    private void Start()
    {
        GoToHome();
    }

    private void ResolveDependencies()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (playerDataManager == null)
        {
            playerDataManager = FindObjectOfType<PlayerDataManager>();
        }

        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }

        if (fishingSceneController == null)
        {
            fishingSceneController = FindObjectOfType<FishingSceneController>();
        }
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
    }

    public void GoToHome()
    {
        SetState(GameState.Home);
        uiManager?.ShowPanel(homePanelName);
    }

    public void GoToFishing(FishingSpot spot, string spotDisplayName)
    {
        if (spot == null)
        {
            Debug.LogError("[GameManager] GoToFishing 调用缺少 FishingSpot。请在按钮或入口中传入有效引用。");
            return;
        }

        if (fishingSceneController == null)
        {
            Debug.LogError("[GameManager] 未能找到 FishingSceneController，无法初始化钓鱼模块。");
            return;
        }

        SetState(GameState.InFishing);
        uiManager?.ShowPanel(fishingPanelName);
        fishingSceneController.PrepareFishingSession(spot, spotDisplayName);
    }

    public void RestartFishing()
    {
        if (CurrentState != GameState.InFishing)
        {
            Debug.LogWarning("[GameManager] 当前不在钓鱼状态，忽略重开请求。");
            return;
        }

        fishingSceneController?.RestartFishingProcess();
    }

    public void GoToFishTank(bool canSell = true)
    {
        SetState(GameState.ViewingFishTank);
        uiManager?.ShowPanel(fishTankPanelName);

        var panel = uiManager?.GetPanel(fishTankPanelName);
        if (panel != null)
        {
            var fishTankUI = panel.GetComponentInChildren<FishTankUIController>(true);
            fishTankUI?.Configure(canSell);
        }
    }

    public void GoToShop()
    {
        SetState(GameState.ViewingShop);
        uiManager?.ShowPanel(shopPanelName);
    }

    public void GoToMap()
    {
        SetState(GameState.ViewingMap);
        uiManager?.ShowPanel(mapPanelName);
    }
}
