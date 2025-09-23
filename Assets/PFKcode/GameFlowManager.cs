using UnityEngine;
using TMPro; // 因为要控制TextMeshPro文本，所以需要引入它的命名空间

public class GameFlowManager : MonoBehaviour
{
    [Header("核心UI面板")]
    public GameObject MainMenuPanel;
    public GameObject FishingUIPanel;

    [Header("动态UI控件")]
    public TextMeshProUGUI SpotTitleText;

    [Header("核心控制器关联")]
    public CastingAndHookingController CastingController;
    public FishingMiniGameManager MiniGameManager; // 我们也需要和小游戏总管对话
    
    // 我们需要知道所有的钓点信息，才能把它们的数据传递下去
    [Header("钓点数据")]
    public FishingSpot CreekSpot;     // 小溪钓点
    public FishingSpot ForestLakeSpot; // 森林湖钓点
    public FishingSpot BeachSpot;      // 沙滩钓点

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
        MainMenuPanel.SetActive(true);
        FishingUIPanel.SetActive(false);
    }

    /// <summary>
    /// “选择小溪钓点”的指令 (这个方法将由按钮调用)
    /// </summary>
    public void SelectCreekSpot()
    {
        // 1. 将钓点数据传递给小游戏总管，让它提前准备好鱼池信息
        MiniGameManager.CurrentFishData = CreekSpot.SelectFishByWeight();

        // 2. 切换界面，并更新标题
        MainMenuPanel.SetActive(false);
        FishingUIPanel.SetActive(true);
        SpotTitleText.text = "小溪";

        // 3. 启动抛竿流程
        CastingController.StartCastingProcess();
    }

    /// <summary>
    /// “选择森林湖钓点”的指令 (这个方法将由按钮调用)
    /// </summary>
    public void SelectForestLakeSpot()
    {
        // 1. 传递数据
        MiniGameManager.CurrentFishData = ForestLakeSpot.SelectFishByWeight();
        
        // 2. 切换界面
        MainMenuPanel.SetActive(false);
        FishingUIPanel.SetActive(true);
        SpotTitleText.text = "森林湖";

        // 3. 启动流程
        CastingController.StartCastingProcess();
    }

    /// <summary>
    /// “选择沙滩钓点”的指令 (这个方法将由按钮调用)
    /// </summary>
    public void SelectBeachSpot()
    {
        // 1. 传递数据
        MiniGameManager.CurrentFishData = BeachSpot.SelectFishByWeight();
        
        // 2. 切换界面
        MainMenuPanel.SetActive(false);
        FishingUIPanel.SetActive(true);
        SpotTitleText.text = "沙滩";

        // 3. 启动流程
        CastingController.StartCastingProcess();
    }
}