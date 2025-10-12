using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using System.Collections; // 引入协程命名空间
using System.Collections.Generic;
using TMPro; // 引入TextMeshPro的命名空间
using UnityEngine.Serialization;

/// <summary>
/// 迷你游戏的管理脚本，控制整个钓鱼博弈的流程、状态和进度。
/// </summary>
public class FishingMiniGameManager : MonoBehaviour
{
    #region 公有变量
    [Header("系统关联")]
    [SerializeField, FormerlySerializedAs("TheGameFlowManager"), FormerlySerializedAs("gameFlowManager")] private FishingSceneController sceneController;
    [SerializeField, FormerlySerializedAs("TreasureChestController")] private TreasureChestController treasureChestController;

    [Tooltip("对库存管理器的引用")]
    [SerializeField, FormerlySerializedAs("TheInventoryManager")] private InventoryManager inventoryManager; // 【新增】对InventoryManager的引用

    [Header("当前钓获物的数据")]
    [SerializeField, FormerlySerializedAs("CurrentCatchableData")] private CatchableData currentCatchableData; // 当前正在钓的“可捕捉物”的配置数据。
    public CatchableData CurrentCatchableData
    {
        get => currentCatchableData;
        set => currentCatchableData = value;
    }

    [Header("游戏对象关联")]
    [SerializeField, FormerlySerializedAs("ChestLootText")] private TextMeshProUGUI chestLootText; // 【新】宝箱战利品文本
    [SerializeField, FormerlySerializedAs("MiniGamePanel")] private GameObject miniGamePanel;        // 整个钓鱼游戏UI的容器
    [SerializeField, FormerlySerializedAs("ProgressBar")] private Slider progressBar;              // 进度条
    [SerializeField, FormerlySerializedAs("PlayerBar")] private RectTransform playerBar;          // 玩家控制的绿条
    [SerializeField, FormerlySerializedAs("ResultStatusText")] private TextMeshProUGUI resultStatusText; // 结果状态文本
    [SerializeField, FormerlySerializedAs("PlayerBarController")] private PlayerBarController playerBarController; // 玩家控制的绿条脚本
    [SerializeField, FormerlySerializedAs("CloseMiniGameButton")] private Button closeMiniGameButton;      // 开始/关闭按钮（虽然在InProgress时隐藏）
    [SerializeField, FormerlySerializedAs("FishController")] private FishController fishController;    // 鱼/可捕捉物的图标脚本

    public FishingSceneController SceneController
    {
        get => sceneController;
        set => sceneController = value;
    }

    public TreasureChestController TreasureChestController => treasureChestController;
    public InventoryManager InventoryManager => inventoryManager;
    public Slider ProgressBar => progressBar;
    public RectTransform PlayerBar => playerBar;
    public PlayerBarController PlayerBarController => playerBarController;
    public FishController FishController => fishController;
    public TextMeshProUGUI ChestLootText => chestLootText;
    public GameObject MiniGamePanel => miniGamePanel;
    public TextMeshProUGUI ResultStatusText => resultStatusText;
    public Button CloseMiniGameButton => closeMiniGameButton;
    public InventoryManager TheInventoryManager => inventoryManager;

    /// <summary> 我们用一个公开的枚举来定义所有可能的游戏状态 </summary>
    public enum GameState
    {
        NotStarted, // 还未开始
        InProgress, // 进行中
        Success,    // 成功
        Failed      // 失败
    }
    public GameState CurrentMiniGameState { get; private set; } // 创建一个变量来存储当前的状态
    public enum FishQuality { 吹牛资本, 史诗对决, 像模像样, 勉强上钩 }

    [Header("结果界面按钮")]
    [SerializeField, FormerlySerializedAs("PutInBackpackButton")] private Button putInBackpackButton; // “放入背包”按钮
    [SerializeField, FormerlySerializedAs("ReturnToMenuButton")] private Button returnToMenuButton;  // “返回主菜单”按钮

    public Button PutInBackpackButton => putInBackpackButton;
    public Button ReturnToMenuButton => returnToMenuButton;

    [Header("游戏参数 - 可在Inspector中调整")]
    [SerializeField, FormerlySerializedAs("ProgressIncreaseRate")] private float progressIncreaseRate = 0.2f; // 进度条增长速率
    [SerializeField, FormerlySerializedAs("ProgressDecreaseRate")] private float progressDecreaseRate = 0.1f; // 进度条衰减速率
    public float ProgressIncreaseRate => progressIncreaseRate;
    public float ProgressDecreaseRate => progressDecreaseRate;
    #endregion

    #region 私有变量
    private float _fishingAreaHeight;       // 钓鱼区域的总高度
    private int _progressDropCount;         // (计数器) 记录进度条下降的次数
    private bool _isCurrentlyOverlapping;   // (状态追踪器) 记录“上一帧”是否在重叠
    private FishingSpot _currentSpot;       // 用来“记住”是哪个钓鱼点启动了我们
    #endregion

    #region unity回调函数
    private void Awake()
    {
        if (sceneController == null)
        {
            sceneController = FindObjectOfType<FishingSceneController>();
        }
    }

    void Start()
    {
        CurrentMiniGameState = GameState.NotStarted;
        // 初始状态下，隐藏所有相关UI
        MiniGamePanel.SetActive(false);
        ResultStatusText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 我们的大脑：只有在“进行中”状态下，才处理这些游戏逻辑
        if (CurrentMiniGameState == GameState.InProgress)
        {
            PlayerBarController.HandleUpdate();
            UpdateProgress();
        }
    }
    #endregion

    #region 公有方法
    /// <summary> 这个方法用来启动小游戏的过场动画和准备工作 </summary>
    public void TriggerMiniGameStartSequence(FishingSpot spot)
    {
        StartCoroutine(StartMiniGameCoroutine(spot));
    }

    /// <summary> 这个方法处理“放入背包”按钮的点击 </summary>
    public void OnResult_PutInBackpack()
    {
        HideResultUI();
        // 向钓鱼场景控制器汇报，请求重新开始钓鱼流程
        if (SceneController != null)
        {
            SceneController.RestartFishingProcess();
        }
        else
        {
            GameManager.Instance?.RestartFishing();
        }
    }

    /// <summary> 这个方法处理“返回主菜单”按钮的点击 </summary>
    public void OnResult_ReturnToMenu()
    {
        HideResultUI();
        if (SceneController != null)
        {
            SceneController.HandleReturnToMenu();
        }
        else
        {
            GameManager.Instance?.GoToHome();
        }
    }
    #endregion

    #region 私有方法

    /// <summary> 这个协程处理小游戏的启动过场 </summary>
    private IEnumerator StartMiniGameCoroutine(FishingSpot spot)
    {
        // --- 第1阶段：准备舞台，显示“上钩了！” ---
        MiniGamePanel.SetActive(true);
        ProgressBar.gameObject.SetActive(false);
        PlayerBar.gameObject.SetActive(false);
        FishController.gameObject.SetActive(false);

        // 显示提示文本
        ResultStatusText.text = "上钩了!";
        ResultStatusText.gameObject.SetActive(true);

        // --- 第2阶段：停顿1秒 ---
        yield return new WaitForSeconds(1f);

        // --- 第3阶段：清理舞台，真正开始游戏 ---
        ResultStatusText.gameObject.SetActive(false);
        InitializeMiniGame(spot);
    }

    /// <summary> 初始化所有游戏UI和参数，正式开始游戏博弈 </summary>
    private void InitializeMiniGame(FishingSpot spot)
    {
        _currentSpot = spot; // 记住是哪个钓鱼点启动了我们

        // --- 【修改2】: 检查通用的 CurrentCatchableData 是否设置 ---
        if (CurrentCatchableData == null)
        {
            Debug.LogError("错误:CurrentCatchableData 未设置！无法开始游戏。");
            return; // 直接退出，不执行后续代码
        }

        ShowGameplayUI();
        CurrentMiniGameState = GameState.InProgress;
        ProgressBar.value = 0.25f; // 设置初始进度
        StopAllCoroutines(); // 停止可能正在“游荡”的旧协程。

        _progressDropCount = 0; // 重置脱钩次数
        _isCurrentlyOverlapping = false; // 游戏开始时默认不在重叠区

        CloseMiniGameButton.gameObject.SetActive(false);
        ResultStatusText.gameObject.SetActive(false);
        FishController.gameObject.SetActive(true);

        // 获取钓鱼区域的高度，用于计算边界
        _fishingAreaHeight = MiniGamePanel.GetComponent<RectTransform>().rect.height;

        // --- 【修改3】: 将通用 CatchableData 传递给 FishController ---
        FishController.Initialize(CurrentCatchableData, _fishingAreaHeight, this);

        // 1. 获取鱼的初始Y坐标
        // 2. 将 fishingAreaHeight 和鱼的初始坐标一起传给 PlayerBarController
        PlayerBarController.Initialize(_fishingAreaHeight, FishController.InitialYPosition);
        FishController.StartBehavior();

    }

    /// <summary> 结束博弈游戏，结算输赢和战利品 </summary>
    private void EndMiniGame(bool success)
    {
        CurrentMiniGameState = success ? GameState.Success : GameState.Failed;

        if (success)
        {
            if (CurrentCatchableData is FishData)
            {
                // 如果是鱼，才执行鱼的品质和长度计算
                var currentFish = CurrentCatchableData as FishData; // 转换为FishData方便访问专属属性

                // 1. 进行品质审判：根据进度条下降次数判断品质
                FishQuality finalQuality;
                if (_progressDropCount == 0) finalQuality = FishQuality.吹牛资本;
                else if (_progressDropCount == 1) finalQuality = FishQuality.史诗对决;
                else if (_progressDropCount == 2) finalQuality = FishQuality.像模像样;
                else finalQuality = FishQuality.勉强上钩;

                // 2. 计算最终长度 (注意：这里调用了新的带 FishData 参数的方法)
                float finalLength = CalculateFishLength(finalQuality, currentFish);

                // 3. 显示鱼的结果
                ResultStatusText.text = $"成功!\n品质: {finalQuality}\n长度: {finalLength:F2} cm";
                Debug.Log($"渔获报告 - 鱼: {currentFish.ItemName} \n品质: {finalQuality},\n长度: {finalLength:F2} cm");
                // 将渔获结果打包，并发送给InventoryManager
                var result = new CatchResult
                {
                    FishedData = currentFish,
                    FishedQuality = finalQuality,
                    Length = finalLength
                };
                // 安全检查，确保TheInventoryManager已经从编辑器关联
                if (TheInventoryManager != null)
                {
                    TheInventoryManager.AddItem(result);
                }
                else
                {
                    Debug.LogError("FishingMiniGameManager中的TheInventoryManager未设置引用!");
                }
                var awardedChest = TreasureChestController.TryToAwardChest();
                if (awardedChest != null)
                {
                    // 如果中奖了，就启动开箱序列的协程
                    StartCoroutine(ChestOpeningSequenceCoroutine(awardedChest));
                }
            }
            else
            {
                // 如果不是鱼（比如是垃圾），就显示通用成功信息
                ResultStatusText.text = $"成功!\n获得了: \n{CurrentCatchableData.ItemName}";
                Debug.Log($"渔获报告 - 物品: {CurrentCatchableData.ItemName}");
            }
        }
        else
        {
            ResultStatusText.text = "失败!";
        }

        // 停止所有游戏内活动
        FishController.StopBehavior();

    // 隐藏游戏UI
        ProgressBar.gameObject.SetActive(false);
        PlayerBar.gameObject.SetActive(false);
        FishController.gameObject.SetActive(false);

        // 显示结果文本和两个新按钮
        ResultStatusText.gameObject.SetActive(true);
        PutInBackpackButton.gameObject.SetActive(true);
        ReturnToMenuButton.gameObject.SetActive(true);

    SceneController?.NotifyFishingSessionEnded();
    }

    /// <summary> 更新进度条 </summary>
    private void UpdateProgress()
    {
        bool isOverlappingNow = IsOverlapping(); // 使用重叠判断函数判断这一帧重叠情况

        // 核心逻辑：检测脱钩状态变化
        // 如果“上一帧在重叠”并且“这一帧没重叠”，说明一次“脱钩”发生了
        if (_isCurrentlyOverlapping && !isOverlappingNow)
        {
            _progressDropCount++;
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

        // 判断输赢 
        if (ProgressBar.value >= 1f)
        {
            EndMiniGame(true);
        }
        else if (ProgressBar.value <= 0f)
        {
            EndMiniGame(false);
        }
    }

    /// <summary> 检查绿条和鱼是否重叠 </summary>
    private bool IsOverlapping()
    {
        float playerBarTop = PlayerBar.anchoredPosition.y + PlayerBar.rect.height / 2;
        float playerBarBottom = PlayerBar.anchoredPosition.y - PlayerBar.rect.height / 2;
        float fishTop = FishController.TopY;
        float fishBottom = FishController.BottomY;

        // 检查两个矩形是否重叠的逻辑
        return playerBarTop > fishBottom && playerBarBottom < fishTop;
    }

    /// <summary>
    /// 根据品质和具体的鱼数据计算最终的鱼长度。
    /// 【修改5】: 此方法现在需要传入具体的 FishData 才能访问长度范围。
    /// </summary>
    private float CalculateFishLength(FishQuality quality, FishData fishData)
    {
        // 根据品质，从传入的 fishData 中选择对应的长度范围
        Vector2 lengthRange;
        switch (quality)
        {
            case FishQuality.吹牛资本:
                lengthRange = fishData.LengthRangeChuiNiuZiBen;
                break;
            case FishQuality.史诗对决:
                lengthRange = fishData.LengthRangeShiShiDuiJue;
                break;
            case FishQuality.像模像样:
                lengthRange = fishData.LengthRangeXiangMoXiangYang;
                break;
            default: // 默认情况，包括 "勉强上钩"
                lengthRange = fishData.LengthRangeMianQiang;
                break;
        }

        // 在选定的范围内，生成一个随机的长度值
        return Random.Range(lengthRange.x, lengthRange.y);
    }

    /// <summary> 隐藏结果界面相关UI </summary>
    private void HideResultUI()
    {
        ResultStatusText.gameObject.SetActive(false);
        PutInBackpackButton.gameObject.SetActive(false);
        ReturnToMenuButton.gameObject.SetActive(false);
        HideGameplayUI();
        if (ChestLootText != null)
        {
            ChestLootText.gameObject.SetActive(false);
        }
    }

    /// <summary> 显示博弈玩法相关UI </summary>
    private void ShowGameplayUI()
    {
        if (MiniGamePanel != null) MiniGamePanel.SetActive(true);
        if (ProgressBar != null) ProgressBar.gameObject.SetActive(true);
        if (PlayerBar != null) PlayerBar.gameObject.SetActive(true);
        if (FishController != null) FishController.gameObject.SetActive(true);
    }

    /// <summary> 隐藏博弈玩法相关UI </summary>
    private void HideGameplayUI()
    {
        if (MiniGamePanel != null) MiniGamePanel.SetActive(false);
    }

    #endregion

    /// <summary> 我们用一个结构体来封装钓鱼结果（现在只能存储 FishData 的结果） </summary>
    public struct CatchResult
    {
        public FishData FishedData; // 钓上来的鱼的原始数据
        public FishingMiniGameManager.FishQuality FishedQuality; // 成品鱼品质
        public float Length; // 根据品质计算出的最终长度
    }
    

    /// 【新】协程：处理宝箱开箱序列
    /// <summary>
    /// 【新】处理宝箱开启流程的协程（动画、UI、音效的预留位）
    /// </summary>
    private IEnumerator ChestOpeningSequenceCoroutine(TreasureChestData chest)
    {
        Debug.Log($"恭喜！额外获得了宝箱：{chest.ItemName}!");
        // --- 这里是未来播放“获得宝箱”动画和音效的地方 ---

        // 1. 等待一段时间，让玩家先看清楚鱼的收获
        yield return new WaitForSeconds(1.0f);

        // 2. 准备并激活宝箱UI
        ChestLootText.gameObject.SetActive(true);
        ChestLootText.rectTransform.localScale = Vector3.zero; // 初始大小为0，为动画做准备

        // 3. 生成战利品列表字符串
        string lootText = $"打开 {chest.ItemName} 获得了:\n";
        int lootCount = Random.Range(chest.LootCountRange.x, chest.LootCountRange.y + 1);
        for (int i = 0; i < lootCount; i++)
        {
            if (chest.LootPool != null && chest.LootPool.Count > 0)
            {
                var randomItem = chest.LootPool[Random.Range(0, chest.LootPool.Count)];// 从战利品池中随机选一个
                lootText += $"- {randomItem.ItemName}\n";// 添加到文本
            }
        }
        ChestLootText.text = lootText;// 显示战利品文本

        // 4. 【简易动画】播放字体由小到大的动画
        float duration = 0.5f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            ChestLootText.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);// 线性插值
            yield return null;//让while循环每帧只执行一次
        }
        ChestLootText.rectTransform.localScale = Vector3.one; // 确保最终是准确大小

        // 5. 再等待几秒，让玩家阅读
        yield return new WaitForSeconds(4.0f);

        // 6. 隐藏UI，结束序列
        ChestLootText.gameObject.SetActive(false);
    }
    
}
