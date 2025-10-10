using UnityEngine;
using TMPro; // 因为要控制TextMeshPro文本，所以需要引入它的命名空间
using UnityEngine.Serialization;

public class GameFlowManager : MonoBehaviour
{
    [Header("核心UI面板")]
    [SerializeField, FormerlySerializedAs("MainMenuPanel")] private GameObject mainMenuPanel;
    [SerializeField, FormerlySerializedAs("FishingUIPanel")] private GameObject fishingUIPanel;

    [Header("动态UI控件")]
    [SerializeField, FormerlySerializedAs("SpotTitleText")] private TextMeshProUGUI spotTitleText;

    [Header("核心控制器关联")]
    [SerializeField, FormerlySerializedAs("CastingController")] private CastingAndHookingController castingController;
    [SerializeField, FormerlySerializedAs("MiniGameManager")] private FishingMiniGameManager miniGameManager; // 我们也需要和小游戏总管对话
    public CastingAndHookingController CastingController => castingController;
    public FishingMiniGameManager MiniGameManager => miniGameManager;
    
    // 我们需要知道所有的钓点信息，才能把它们的数据传递下去
    [Header("钓点数据")]
    [SerializeField, FormerlySerializedAs("CreekSpot")] private FishingSpot creekSpot;     // 小溪钓点
    [SerializeField, FormerlySerializedAs("ForestLakeSpot")] private FishingSpot forestLakeSpot; // 森林湖钓点
    [SerializeField, FormerlySerializedAs("BeachSpot")] private FishingSpot beachSpot;      // 沙滩钓点

    void Start()
    {
        // 游戏一启动，就回到主菜单
        GoToMainMenu();
    }

    /// <summary>
    /// “回到主菜单”的指令
    /// </summary>
    public void GoToMainMenu()
    {
    mainMenuPanel.SetActive(true);
    fishingUIPanel.SetActive(false);
    }

    /// <summary>
    /// “选择小溪钓点”的指令 (这个方法将由按钮调用)
    /// </summary>
    public void SelectCreekSpot()
    {
    PrepareToFish(creekSpot, "小溪");
    }

    public void SelectForestLakeSpot()
    {
    PrepareToFish(forestLakeSpot, "森林湖");
    }

    public void SelectBeachSpot()
    {
    PrepareToFish(beachSpot, "沙滩");
    }

    public void RestartFishingProcess()//重新开始钓鱼流程
    {
        // 重新开始抛竿流程
    castingController.StartCastingProcess();
    }
    // 我们把重复的逻辑抽出来，放到一个私有方法里
    private void PrepareToFish(FishingSpot spot, string spotName)//
    {
        var selectedItem = spot.SelectItemByWeight(); // 使用新方法名
        // 无论是鱼、垃圾还是宝箱，它们都是CatchableData，直接传递给小游戏总管
                miniGameManager.CurrentCatchableData = selectedItem;
                mainMenuPanel.SetActive(false);//隐藏主菜单
                fishingUIPanel.SetActive(true);
                spotTitleText.text = spotName;

        // 启动带延迟的协程，而不是直接调用
        StartCoroutine(DelayedStartCasting());
    }
        public void GoToFishTank()
    {
        // TODO: 阶段二实现
        // MainMenuPanel.SetActive(false);
        // FishingUIPanel.SetActive(false);
        // FishTankPanel.SetActive(true);
        Debug.Log("【GameFlowManager】切换到鱼箱界面。");
    }

    // 新增一个协程，用于延迟调用
    private System.Collections.IEnumerator DelayedStartCasting()
    {
        // 等待一帧，让点击事件过去
        yield return null;

        // 在下一帧再真正开始抛竿流程
    castingController.StartCastingProcess();
    }
    public void GoToFishTank(bool canSell)
    {
        // TODO: 阶段二实现
        // 1. 隐藏其他面板，显示鱼箱面板
        // 2. 获取鱼箱面板上的FishTankUIController组件
        // 3. 调用其Initialize(canSell)方法
        Debug.Log($"【GameFlowManager】切换到鱼箱, 是否可出售: {canSell}");
    }

    public void GoToShop()
    {
        // TODO: 阶段二实现
        Debug.Log("【GameFlowManager】切换到商店。");
    }

    public void GoToHome()
    {
        // TODO: 阶段二实现
        Debug.Log("【GameFlowManager】切换到家。");
    }

    public void ShowMapView()
    {
        // TODO: 阶段二实现
        Debug.Log("【GameFlowManager】显示地图选择界面。");
    }
}