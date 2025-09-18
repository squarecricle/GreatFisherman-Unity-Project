using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using System.Collections; // 引入协程命名空间
using TMPro; // 引入TextMeshPro的命名空间

public class FishingGameManager : MonoBehaviour
{
    [Header("当前鱼的数据")]
    public FishData currentFishData;
    [Header("游戏对象关联")]
    public GameObject fishingGamePanel; // 整个钓鱼游戏UI的容器
    public Slider progressBar;          // 进度条
    public RectTransform playerBar;     // 玩家控制的绿条
    public RectTransform fishIcon;      // 鱼的图标
    public TextMeshProUGUI statusText; // 把 Text 修改为 TextMeshProUGUI

    public Button startButton;          // 开始按钮
    public enum GameState// 我们用一个公开的枚举来定义所有可能的游戏状态 
    {
        NotStarted, // 还未开始
        InProgress, // 进行中
        Success,    // 成功
        Failed      // 失败
    }
    public GameState currentState; // 创建一个变量来存储当前的状态
    public enum FishQuality { 吹牛资本, 史诗对决, 像模像样, 勉强上钩 }

    [Header("游戏参数 - 可在Inspector中调整")]
    public float playerBarMoveSpeed = 300f; // 绿条上升速度
    public float gravity = 800f;            // 绿条受到的重力
    public float progressIncreaseRate = 0.2f; // 进度条增长速率
    public float progressDecreaseRate = 0.1f; // 进度条衰减速率

/////////私有变量区域vvvvvv
    private float playerBarVerticalVelocity = 0f; // 绿条当前的垂直速度
    private float fishTargetY;                    // 鱼的目标Y坐标
    private float fishingAreaHeight;              // 钓鱼区域的总高度
    private float _fishMinY;                    // (缓存) 鱼活动的最小Y坐标
    private float _fishMaxY;                    // (缓存) 鱼活动的最大Y坐标
    private int _progressDropCount; // (计数器) 记录进度条下降的次数
    private bool _isCurrentlyOverlapping; // (状态追踪器) 记录“上一帧”是否在重叠

////////私有变量区域///////

    void Start()
    {
        currentState = GameState.NotStarted;

        // 游戏开始时，隐藏钓鱼UI，只显示开始按钮
        fishingGamePanel.SetActive(false);
        statusText.gameObject.SetActive(false);
        startButton.onClick.AddListener(StartGame); // 为按钮添加点击事件
    }

    public void StartGame()
    {
        //防止忘记在Inspector里拖拽currentFishData导致报错
         if (currentFishData == null)
        {
            Debug.LogError("错误：currentFishData 未设置！无法开始游戏。");
            return; // 直接退出，不执行后续代码
        }

        // 初始化游戏状态
        currentState = GameState.InProgress;
        progressBar.value = 0.25f; // 设置初始进度
        StopAllCoroutines();//这可以防止在快速连续开始/结束游戏时，有旧的协程还在“游荡”。
                            // 显示游戏UI，隐藏按钮和状态文本
       
        _progressDropCount = 0;//重置脱钩次数
        _isCurrentlyOverlapping = false; // 游戏开始时默认不在重叠区

        fishingGamePanel.SetActive(true);
        startButton.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);

        // 获取钓鱼区域的高度，用于计算边界
        fishingAreaHeight = fishingGamePanel.GetComponent<RectTransform>().rect.height;

        // 初始化玩家条和鱼的位置
        playerBar.anchoredPosition = new Vector2(playerBar.anchoredPosition.x, 0);
        fishIcon.anchoredPosition = new Vector2(fishIcon.anchoredPosition.x, 0);

        // 启动鱼的移动逻辑
        StartCoroutine(FishBehavior());
        //代码优化钓鱼小游戏前先计算边界范围
        float halfFishHeight = fishIcon.rect.height / 2;
        _fishMinY = -fishingAreaHeight / 2 + halfFishHeight;
        _fishMaxY = fishingAreaHeight / 2 - halfFishHeight;        
    }

    void Update()
    {
        // 我们的大脑：根据当前是什么状态，就做什么事
        switch (currentState)
        {
            case GameState.InProgress:
                // 只有在“进行中”状态下，才处理这些游戏逻辑
                HandlePlayerInput();
                MovePlayerBar();
                MoveFish();
                UpdateProgress();
                break;
                // 以后我们可以在这里添加其他状态的逻辑，比如成功时播放庆祝动画等
                // case GameState.Success:
                //     // Play celebration animation...
                //     break;
        }
    }

    // 1. 处理玩家输入
    void HandlePlayerInput()
    {
        if (Input.GetMouseButton(0)) // 鼠标左键按住 (在手机上对应触摸)
        {
            // 按住时，给一个向上的速度
            playerBarVerticalVelocity = playerBarMoveSpeed;
        }
        else
        {
            // 松开时，只受重力影响，速度会持续下降
            playerBarVerticalVelocity -= gravity * Time.deltaTime;
        }
    }

    // 2. 移动玩家的绿条
    void MovePlayerBar()
    {
        // 根据速度更新位置
        playerBar.anchoredPosition += new Vector2(0, playerBarVerticalVelocity * Time.deltaTime);

        // 限制绿条不出界
        float halfPlayerBarHeight = playerBar.rect.height / 2;
        float minY = -fishingAreaHeight / 2 + halfPlayerBarHeight;
        float maxY = fishingAreaHeight / 2 - halfPlayerBarHeight;

        float currentY = playerBar.anchoredPosition.y;
        currentY = Mathf.Clamp(currentY, minY, maxY); // Mathf.Clamp是限制范围的神器
        playerBar.anchoredPosition = new Vector2(playerBar.anchoredPosition.x, currentY);
    }

    // 3. 移动鱼 (使用协程控制行为模式)
    IEnumerator FishBehavior()
    {
        // 我们需要一个循环，只要游戏还在进行中，这个循环就一直执行
        while (currentState == GameState.InProgress)
        {
            switch (currentFishData.behaviorType)
            {
                case FishData.FishBehaviorType.平滑移动:

                    // 随机一个新的目标Y位置

                    fishTargetY = Random.Range(_fishMinY, _fishMaxY);

                    // 随机一个等待时间，模拟鱼的思考
                    // 协程会在这里暂停，但因为外层有while循环，
                    // 所以当它恢复时，会再次回到while的条件判断，开始下一次循环
                    yield return new WaitForSeconds(Random.Range(currentFishData.minPauseDuration, currentFishData.maxPauseDuration));
                    //随机生成[目前鱼数据]里的等待时间区间的某个时间，模拟鱼的思考，等待该时间后协程再次开启
                    break;
            }
         }
    }

    void MoveFish()
    {
        // 平滑地将鱼移动到目标位置
        Vector2 currentFishPos = fishIcon.anchoredPosition;//读取这上一帧最后鱼的所在位置
        Vector2 targetPos = new Vector2(currentFishPos.x, fishTargetY);//读取协程中新生成的位置 
        fishIcon.anchoredPosition = Vector2.MoveTowards(currentFishPos, targetPos, currentFishData.moveSpeed * Time.deltaTime);
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
        progressBar.value += progressIncreaseRate * Time.deltaTime;
    }
    else
    {
        progressBar.value -= progressDecreaseRate * Time.deltaTime;
    }

    // 判断输赢 (这部分逻辑不变)
    if (progressBar.value >= 1f)
    {
        EndGame(true);
    }
    else if (progressBar.value <= 0f)
    {
        EndGame(false);
    }
}
    // 检查绿条和鱼是否重叠
    bool IsOverlapping()
    {
        float playerBarTop = playerBar.anchoredPosition.y + playerBar.rect.height / 2;
        float playerBarBottom = playerBar.anchoredPosition.y - playerBar.rect.height / 2;
        float fishTop = fishIcon.anchoredPosition.y + fishIcon.rect.height / 2;
        float fishBottom = fishIcon.anchoredPosition.y - fishIcon.rect.height / 2;

        // 如果绿条的顶部在鱼的底部之上，或者绿条的底部在鱼的顶部之下，则没有重叠
        // 所以反过来，就是重叠了
        return playerBarTop > fishBottom && playerBarBottom < fishTop;
    }

    void EndGame(bool success)
    {
        // 根据游戏结果，设置不同的结束状态
        if (success)
        {
            currentState = GameState.Success;
        }
        else
        {
            currentState = GameState.Failed;
        }
        fishingGamePanel.SetActive(false);
        statusText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);

        if (success)
        {
            statusText.text = "成功!";
            // 在这里可以添加成功后的奖励逻辑
        }
        else
        {
            statusText.text = "失败!";
        }
    }
}