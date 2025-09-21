using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using System.Collections; // 引入协程命名空间
using TMPro; // 引入TextMeshPro的命名空间

public class FishingGameManager : MonoBehaviour
{
    #region 公有变量
    [Header("当前鱼的数据")]
    public FishData CurrentFishData;
    [Header("游戏对象关联")]
    public GameObject MiniGamePanel; // 整个钓鱼游戏UI的容器
    public Slider ProgressBar;          // 进度条
    public RectTransform PlayerBar;     // 玩家控制的绿条
    public RectTransform FishIcon;      // 鱼的图标
    public TextMeshProUGUI ResultStatusText; // 把 Text 修改为 TextMeshProUGUI
    public PlayerBarController PlayerBarController; // 玩家控制的绿条脚本
    public Button CloseMiniGameButton;          // 开始按钮
    public enum GameState// 我们用一个公开的枚举来定义所有可能的游戏状态 
    {
        NotStarted, // 还未开始
        InProgress, // 进行中
        Success,    // 成功
        Failed      // 失败
    }
    public GameState CurrentMiniGameState; // 创建一个变量来存储当前的状态
    public enum FishQuality { 吹牛资本, 史诗对决, 像模像样, 勉强上钩 }

    [Header("游戏参数 - 可在Inspector中调整")]


    public float ProgressIncreaseRate = 0.2f; // 进度条增长速率
    public float ProgressDecreaseRate = 0.1f; // 进度条衰减速率
    #endregion
    #region 私有变量

    private float _fishTargetY;                    // 鱼的目标Y坐标
    private float _fishingAreaHeight;              // 钓鱼区域的总高度
    private float _fishMinY;                    // (缓存) 鱼活动的最小Y坐标
    private float _fishMaxY;                    // (缓存) 鱼活动的最大Y坐标
    private int _progressDropCount; // (计数器) 记录进度条下降的次数
    private bool _isCurrentlyOverlapping; // (状态追踪器) 记录“上一帧”是否在重叠
    private FishingSpot _currentSpot; // (新增) 用来“记住”是哪个钓鱼点启动了我们    
    #endregion
    #region 私有函数
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



    #endregion
        void Start()
    {
        CurrentMiniGameState = GameState.NotStarted;

        // 初始状态下，隐藏所有相关UI
        MiniGamePanel.SetActive(false);
        ResultStatusText.gameObject.SetActive(false);
        
        CloseMiniGameButton.onClick.AddListener(CloseMiniGame); // <--- 必须添加这一行！

        CloseMiniGameButton.gameObject.SetActive(false); 
       
    }

    public void StartMiniGame(FishingSpot spot)
    {
        _currentSpot = spot; // <--- 添加这一行，把传进来的spot存起来
        //防止忘记在Inspector里拖拽currentFishData导致报错
        if (CurrentFishData == null)
        {
            Debug.LogError("错误：currentFishData 未设置！无法开始游戏。");
            return; // 直接退出，不执行后续代码
        }

        // 初始化游戏状态
        CurrentMiniGameState = GameState.InProgress;
        ProgressBar.value = 0.25f; // 设置初始进度
        StopAllCoroutines();//这可以防止在快速连续开始/结束游戏时，有旧的协程还在“游荡”。
                            // 显示游戏UI，隐藏按钮和状态文本

        _progressDropCount = 0;//重置脱钩次数
        _isCurrentlyOverlapping = false; // 游戏开始时默认不在重叠区

        MiniGamePanel.SetActive(true);
        CloseMiniGameButton.gameObject.SetActive(false);
        ResultStatusText.gameObject.SetActive(false);

        // 获取钓鱼区域的高度，用于计算边界
        _fishingAreaHeight = MiniGamePanel.GetComponent<RectTransform>().rect.height;
        PlayerBarController.Initialize(_fishingAreaHeight); // 初始化玩家条的位置和范围
        // 初始化鱼的位置
        FishIcon.anchoredPosition = new Vector2(FishIcon.anchoredPosition.x, 0);

        // 启动鱼的移动逻辑
        StartCoroutine(FishBehavior());
        //代码优化钓鱼小游戏前先计算边界范围
        float halfFishHeight = FishIcon.rect.height / 2;
        _fishMinY = -_fishingAreaHeight / 2 + halfFishHeight;
        _fishMaxY = _fishingAreaHeight / 2 - halfFishHeight;
    }

    void Update()
    {
        // 我们的大脑：根据当前是什么状态，就做什么事
        switch (CurrentMiniGameState)
        {
            case GameState.InProgress:
                // 只有在“进行中”状态下，才处理这些游戏逻辑
                PlayerBarController.HandleUpdate(); // ←-- 修改这里
                MoveFish();
                UpdateProgress();
                break;
                // 以后我们可以在这里添加其他状态的逻辑，比如成功时播放庆祝动画等
                // case GameState.Success:
                //     // Play celebration animation...
                //     break;
        }
    }

    // 3. 移动鱼 (使用协程控制行为模式)
    IEnumerator FishBehavior()
    {
        // 我们需要一个循环，只要游戏还在进行中，这个循环就一直执行
        while (CurrentMiniGameState == GameState.InProgress)
        {
            switch (CurrentFishData.FishBehavior)
            {
                case FishData.FishBehaviorType.平滑移动:

                    // 随机一个新的目标Y位置

                    _fishTargetY = Random.Range(_fishMinY, _fishMaxY);

                    // 随机一个等待时间，模拟鱼的思考
                    // 协程会在这里暂停，但因为外层有while循环，
                    // 所以当它恢复时，会再次回到while的条件判断，开始下一次循环
                    yield return new WaitForSeconds(Random.Range(CurrentFishData.MinPauseDuration, CurrentFishData.MaxPauseDuration));
                    //随机生成[目前鱼数据]里的等待时间区间的某个时间，模拟鱼的思考，等待该时间后协程再次开启
                break;
                    // --- 防御性代码 ---
                default:
                    //打印一条清晰的错误日志，告诉未来的我们问题出在哪
                    Debug.LogError($"[FishingGameManager] 遇到未处理的鱼行为类型: {CurrentFishData.FishBehavior}，来自鱼类: {CurrentFishData.FishName}。请检查FishData配置！");

                    // 第二步：提供一个安全的“保底”行为，防止无限循环
                    // 这里我们让它像“平滑移动”一样，只等待，不做任何移动
                    // yield return null; 也是一个选项，代表“暂停一帧”
                    yield return new WaitForSeconds(1f); 
                    break;
                // --- 防御性代码结束 ---
            }
        }
    }

    void MoveFish()
    {
        // 平滑地将鱼移动到目标位置
        Vector2 currentFishPos = FishIcon.anchoredPosition;//读取这上一帧最后鱼的所在位置
        Vector2 targetPos = new Vector2(currentFishPos.x, _fishTargetY);//读取协程中新生成的位置 
        FishIcon.anchoredPosition = Vector2.MoveTowards(currentFishPos, targetPos, CurrentFishData.MoveSpeed * Time.deltaTime);
    }

    // 4. 更新进度条并判断输赢
    void UpdateProgress()
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
    // 检查绿条和鱼是否重叠
    bool IsOverlapping()
    {
        float playerBarTop = PlayerBar.anchoredPosition.y + PlayerBar.rect.height / 2;
        float playerBarBottom = PlayerBar.anchoredPosition.y - PlayerBar.rect.height / 2;
        float fishTop = FishIcon.anchoredPosition.y + FishIcon.rect.height / 2;
        float fishBottom = FishIcon.anchoredPosition.y - FishIcon.rect.height / 2;

        // 如果绿条的顶部在鱼的底部之上，或者绿条的底部在鱼的顶部之下，则没有重叠
        // 所以反过来，就是重叠了
        return playerBarTop > fishBottom && playerBarBottom < fishTop;
    }

    void EndMiniGame(bool success)
    {
        CurrentMiniGameState = success ? GameState.Success : GameState.Failed;
        // 根据游戏结果，设置不同的结束状态
        if (success)
        {
            // 1. 先进行品质审判
            FishQuality finalQuality;//最终品质
            if (_progressDropCount == 0)finalQuality = FishQuality.吹牛资本;
            else if (_progressDropCount == 1)finalQuality = FishQuality.史诗对决;
            else if (_progressDropCount == 2) finalQuality = FishQuality.像模像样;
            else finalQuality = FishQuality.勉强上钩;
            // 2. 计算最终长度
            float finalLength = CalculateFishLength(finalQuality);
            // 3. 创建并打包标准化的“渔获报告”
            CatchResult result = new CatchResult
            {
                FishedData = CurrentFishData,
                FishedQuality = finalQuality,
                Length = finalLength
            };
            // 4. 显示结果
            ResultStatusText.text = $"成功!\n品质: {result.FishedQuality}\n长度: {result.Length:F2} cm"; // 使用了字符串插值和格式化
            Debug.Log($"渔获报告 - 鱼: {result.FishedData.FishName}, 品质: {result. FishedQuality}, 长度: {result.Length:F2} cm");
        }
        else
        {
            ResultStatusText.text = "失败!";
        }


        // 显示结果，隐藏游戏UI，显示按钮和状态文本
        MiniGamePanel.SetActive(false);//隐藏钓鱼UI
        ResultStatusText.gameObject.SetActive(true);//显示结果文本
        CloseMiniGameButton.gameObject.SetActive(true);//显示关闭按钮

        StopAllCoroutines(); // 停止鱼的移动协程
    }
        /// <summary>
        /// 标准化的渔获报告，用于封装一次钓鱼的最终结果
        /// </summary>
        public struct CatchResult //我们用一个结构体来封装钓鱼结果
        {
            public FishData FishedData; // 钓上来的鱼的原始数据
            public FishingGameManager.FishQuality FishedQuality; // 成品鱼品质    
            public float Length; // 根据品质计算出的最终长度
        }    

        // 添加关闭小游戏的方法
    private void CloseMiniGame() // (最好也把它改成private，因为它只应该由按钮在内部调用)
    {
        // 隐藏所有UI元素
        ResultStatusText.gameObject.SetActive(false);
        CloseMiniGameButton.gameObject.SetActive(false);
        CurrentMiniGameState = GameState.NotStarted;

        // 通过之前保存的 _currentSpot，调用它的回调方法，让“开始钓鱼”按钮回来
        if (_currentSpot != null)
        {
            _currentSpot.OnFishingSessionEnd();
        }
    }
}