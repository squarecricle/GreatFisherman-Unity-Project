using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using System.Collections; // 引入协程命名空间
using TMPro; // 引入TextMeshPro的命名空间

public class FishingMiniGameManager : MonoBehaviour
{
    #region 公有变量
    [Header("当前鱼的数据")]
    public FishData CurrentFishData;
    [Header("游戏对象关联")]
    public GameFlowManager TheGameFlowManager;
    public GameObject MiniGamePanel; // 整个钓鱼游戏UI的容器
    public Slider ProgressBar;          // 进度条
    public RectTransform PlayerBar;     // 玩家控制的绿条
    public TextMeshProUGUI ResultStatusText; // 把 Text 修改为 TextMeshProUGUI
    public PlayerBarController PlayerBarController; // 玩家控制的绿条脚本
    public Button CloseMiniGameButton;          // 开始按钮
    public FishController FishController; // 鱼的图标脚本
    public enum GameState// 我们用一个公开的枚举来定义所有可能的游戏状态 
    {
        NotStarted, // 还未开始
        InProgress, // 进行中
        Success,    // 成功
        Failed      // 失败
    }
    public GameState CurrentMiniGameState; // 创建一个变量来存储当前的状态
    public enum FishQuality { 吹牛资本, 史诗对决, 像模像样, 勉强上钩 }
    [Header("结果界面按钮")]
    public Button PutInBackpackButton; // 新增：“放入背包”按钮
    public Button ReturnToMenuButton;  // 新增：“主菜单”按钮
    [Header("游戏参数 - 可在Inspector中调整")]

    public float ProgressIncreaseRate = 0.2f; // 进度条增长速率
    public float ProgressDecreaseRate = 0.1f; // 进度条衰减速率
    #endregion
    #region 私有变量

    private float _fishingAreaHeight;              // 钓鱼区域的总高度
    private float _fishMinY;                    // (缓存) 鱼活动的最小Y坐标
    private float _fishMaxY;                    // (缓存) 鱼活动的最大Y坐标
    private int _progressDropCount; // (计数器) 记录进度条下降的次数
    private bool _isCurrentlyOverlapping; // (状态追踪器) 记录“上一帧”是否在重叠
    private FishingSpot _currentSpot; // (新增) 用来“记住”是哪个钓鱼点启动了我们    
    #endregion
    #region unity回调函数
    void Start()
    {
        CurrentMiniGameState = GameState.NotStarted;

        // 初始状态下，隐藏所有相关UI
        MiniGamePanel.SetActive(false);
        ResultStatusText.gameObject.SetActive(false);

    }
    void Update()
    {
        // 我们的大脑：根据当前是什么状态，就做什么事
        switch (CurrentMiniGameState)
        {
            case GameState.InProgress:
                // 只有在“进行中”状态下，才处理这些游戏逻辑
                PlayerBarController.HandleUpdate(); // ←-- 修改这里
                UpdateProgress();
                break;
                // 以后我们可以在这里添加其他状态的逻辑，比如成功时播放庆祝动画等
                // case GameState.Success:
                //     // Play celebration animation...
                //     break;
        }
    }

    #endregion unity回调函数
    #region 公有方法
    public void TriggerMiniGameStartSequence(FishingSpot spot)//这个方法用来启动小游戏的过场动画和准备工作
    {
        StartCoroutine(StartMiniGameCoroutine(spot));
    }
    public void OnResult_PutInBackpack()//这个方法处理“放入背包”按钮的点击
    {
        // 1. 隐藏自己的结果UI
        HideResultUI();
        // 2. 向总管汇报，请求重新开始
        TheGameFlowManager.RestartFishingProcess();
    }
    public void OnResult_ReturnToMenu()//这个方法处理“返回主菜单”按钮的点击
    {
        // 1. 隐藏自己的结果UI
        HideResultUI();
        // 2. 向总管汇报，请求回到主菜单
        TheGameFlowManager.GoToMainMenu();
    }

    #endregion 公有方法
    #region 私有方法
    private IEnumerator StartMiniGameCoroutine(FishingSpot spot)//这个协程处理小游戏的启动过场
    {
        // --- 第1阶段：准备舞台，显示“上钩了！” ---
        // 激活主面板，但暂时不显示里面的具体游戏UI
        MiniGamePanel.SetActive(true);
        ProgressBar.gameObject.SetActive(false);
        PlayerBar.gameObject.SetActive(false);
        FishController.gameObject.SetActive(false);

        // 显示提示文本
        ResultStatusText.text = "上钩了!";
        ResultStatusText.gameObject.SetActive(true);

        // --- 第2阶段：按你要求，停顿1秒 ---
        yield return new WaitForSeconds(1f);

        // --- 第3阶段：清理舞台，真正开始游戏 ---
        ResultStatusText.gameObject.SetActive(false);

        // 调用我们刚才改好名字的初始化方法，正式布置游戏场景和数据
        InitializeMiniGame(spot);
    }
    private void InitializeMiniGame(FishingSpot spot)
    {
        _currentSpot = spot; // <--- 添加这一行，把传进来的spot存起来
        //防止忘记在Inspector里拖拽currentFishData导致报错
        if (CurrentFishData == null)
        {
            Debug.LogError("错误：currentFishData 未设置！无法开始游戏。");
            return; // 直接退出，不执行后续代码
        }
        // 先激活设置对象的显示
        ShowGameplayUI();
        // 再初始化游戏状态
        CurrentMiniGameState = GameState.InProgress;
        ProgressBar.value = 0.25f; // 设置初始进度
        StopAllCoroutines();//这可以防止在快速连续开始/结束游戏时，有旧的协程还在“游荡”。
                            // 显示游戏UI，隐藏按钮和状态文本

        _progressDropCount = 0;//重置脱钩次数
        _isCurrentlyOverlapping = false; // 游戏开始时默认不在重叠区

        MiniGamePanel.SetActive(true);//显示钓鱼UI
        CloseMiniGameButton.gameObject.SetActive(false);//隐藏开始按钮
        ResultStatusText.gameObject.SetActive(false);//隐藏状态文本
        FishController.gameObject.SetActive(true);//显示鱼
        // 获取钓鱼区域的高度，用于计算边界
        _fishingAreaHeight = MiniGamePanel.GetComponent<RectTransform>().rect.height;
        PlayerBarController.Initialize(_fishingAreaHeight); // 初始化玩家条的位置和范围
        // 初始化鱼的位置
        FishController.Initialize(CurrentFishData, _fishingAreaHeight, this); // <--- 初始化鱼
        FishController.StartBehavior();

    }
    private void EndMiniGame(bool success)//结束博弈游戏
    {
        CurrentMiniGameState = success ? GameState.Success : GameState.Failed;

        // （这部分显示结果文本的逻辑不变）
        if (success)
        {
            // 1. 先进行品质审判
            FishQuality finalQuality;
            if (_progressDropCount == 0) finalQuality = FishQuality.吹牛资本;
            else if (_progressDropCount == 1) finalQuality = FishQuality.史诗对决;
            else if (_progressDropCount == 2) finalQuality = FishQuality.像模像样;
            else finalQuality = FishQuality.勉强上钩;

            // 2. 计算最终长度
            float finalLength = CalculateFishLength(finalQuality);

            // 3. 创建并打包标准化的“渔获报告” (这里是修正后的完整代码)
            CatchResult result = new CatchResult
            {
                FishedData = CurrentFishData,
                FishedQuality = finalQuality,
                Length = finalLength
            };

            // 4. 显示结果
            ResultStatusText.text = $"成功!\n品质: {result.FishedQuality}\n长度: {result.Length:F2} cm";
            Debug.Log($"渔获报告 - 鱼: {result.FishedData.FishName}, 品质: {result.FishedQuality}, 长度: {result.Length:F2} cm");
        }
        else
        {
            ResultStatusText.text = "失败!";
        }

        // --- 核心修改 ---
        // 停止所有游戏内活动
        FishController.StopBehavior();
        StopAllCoroutines();
        ProgressBar.gameObject.SetActive(false);
        PlayerBar.gameObject.SetActive(false);
        FishController.gameObject.SetActive(false);
        // 显示结果文本和两个新按钮
        ResultStatusText.gameObject.SetActive(true);
        PutInBackpackButton.gameObject.SetActive(true);
        ReturnToMenuButton.gameObject.SetActive(true);
    }
    private void UpdateProgress()//更新进度条
    {
        bool isOverlappingNow = IsOverlapping();//使用重叠判断函数判断这一帧重叠情况

        // 核心逻辑：检测状态变化
        // 如果“上一帧在重叠”并且“这一帧没重叠”，说明一次“脱钩”发生了
        if (_isCurrentlyOverlapping && !isOverlappingNow)
        {
            _progressDropCount++;
            Debug.Log("脱钩发生！当前下降次数: " + _progressDropCount); // 添加日志方便我们调试
        }

        // 更新状态记录器，为下一帧做准备
        _isCurrentlyOverlapping = isOverlappingNow;

        // 根据当前是否重叠，更新进度条
        if (isOverlappingNow)
        {
            ProgressBar.value += ProgressIncreaseRate * Time.deltaTime;
        }
        else
        {
            ProgressBar.value -= ProgressDecreaseRate * Time.deltaTime;
        }

        // 判断输赢 (这部分逻辑不变)
        if (ProgressBar.value >= 1f)
        {
            EndMiniGame(true);
        }
        else if (ProgressBar.value <= 0f)
        {
            EndMiniGame(false);
        }
    }
    private bool IsOverlapping()    // 检查绿条和鱼是否重叠
    {
        float playerBarTop = PlayerBar.anchoredPosition.y + PlayerBar.rect.height / 2;
        float playerBarBottom = PlayerBar.anchoredPosition.y - PlayerBar.rect.height / 2;
        float fishTop = FishController.TopY;
        float fishBottom = FishController.BottomY;
        // 如果绿条的顶部在鱼的底部之上，或者绿条的底部在鱼的顶部之下，则没有重叠
        // 所以反过来，就是重叠了
        return playerBarTop > fishBottom && playerBarBottom < fishTop;
    }
    private float CalculateFishLength(FishQuality quality)
    {
        // 根据品质，从 currentFishData 中选择对应的长度范围
        Vector2 lengthRange;
        switch (quality)
        {
            case FishQuality.吹牛资本:
                lengthRange = CurrentFishData.LengthRangeChuiNiuZiBen;
                break;
            case FishQuality.史诗对决:
                lengthRange = CurrentFishData.LengthRangeShiShiDuiJue;
                break;
            case FishQuality.像模像样:
                lengthRange = CurrentFishData.LengthRangeXiangMoXiangYang;
                break;
            default: // 默认情况，包括 "勉强上钩"
                lengthRange = CurrentFishData.LengthRangeMianQiang;
                break;
        }

        // 在选定的范围内，生成一个随机的长度值
        return Random.Range(lengthRange.x, lengthRange.y);
    }
    private void HideResultUI()//隐藏结果界面相关UI
    {
        ResultStatusText.gameObject.SetActive(false);
        PutInBackpackButton.gameObject.SetActive(false);
        ReturnToMenuButton.gameObject.SetActive(false);
        HideGameplayUI();
    }
    private void ShowGameplayUI()//显示博弈玩法相关UI
    {
        if (MiniGamePanel != null) MiniGamePanel.SetActive(true);
        if (ProgressBar != null) ProgressBar.gameObject.SetActive(true);
        if (PlayerBar != null) PlayerBar.gameObject.SetActive(true);
        if (FishController != null) FishController.gameObject.SetActive(true);
    }
    private void HideGameplayUI()//隐藏博弈玩法相关UI
    {
        // 这里我们直接隐藏父级容器，效率更高，也更安全
        // 子对象（进度条、玩家条等）会自动被隐藏
        if (MiniGamePanel != null) MiniGamePanel.SetActive(false);
    }
    
    #endregion 私有方法

    public struct CatchResult //我们用一个结构体来封装钓鱼结果
    {
        public FishData FishedData; // 钓上来的鱼的原始数据
        public FishingMiniGameManager.FishQuality FishedQuality; // 成品鱼品质    
        public float Length; // 根据品质计算出的最终长度
    }
}