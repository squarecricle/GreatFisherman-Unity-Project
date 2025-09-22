using UnityEngine;
using UnityEngine.UI; // 我们会用到UI组件，所以提前引入

public class CastingAndHookingController : MonoBehaviour
{
    // --- 公有变量 (Public Variables) ---
    // 我们预先把未来会用到的UI组件引用放在这里
    [Header("UI组件关联")]
    public GameObject CastingAndHookingPanel; // 整个玩法的UI容器
    public Image SweetSpot;                   // 甜蜜点区域
    public ParabolicPowerBarController PowerBarController; // 蓄力条控制器脚本
    #region 私有变量
    private enum GameplayState
    {
        Inactive,       // 未激活
        Casting,        // 正在蓄力
        WaitingForBite, // 等待咬钩
        Hooking,        // 提线反应
        Success,        // 提钩成功
        Failed          // 提钩失败
    }
    private GameplayState _currentState; // 存储当前所处的状态
    #endregion 私有变量
    // --- Unity生命周期函数 ---
    void Start()
    {
        // 初始状态下，整个UI是隐藏的
        CastingAndHookingPanel.SetActive(false);
        _currentState = GameplayState.Inactive;
        // 【临时测试代码】游戏一开始直接进入抛竿，方便我们看到效果
        // 等功能完成后可以删除这一行
        StartCastingProcess();
    }

    void Update()
    {
        // 这是状态机的核心：每一帧都根据当前状态，执行不同的逻辑
        switch (_currentState)
        {
            case GameplayState.Inactive:
                // 不做任何事
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

    // --- 公有方法 (Public Methods) ---
    // 这是提供给外部的入口，比如给一个“开始钓鱼”的按钮调用
    public void StartCastingProcess()//开始蓄力流程
    {
        CastingAndHookingPanel.SetActive(true);//显示钓鱼UI
        ChangeState(GameplayState.Casting);//切换到蓄力状态
    }

    // --- 私有方法 (Private Methods) ---
    // 一个专门用来改变状态的方法，让逻辑更集中
    private void ChangeState(GameplayState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        Debug.Log("状态切换为: " + _currentState); // 打印日志方便我们调试
    }
    private void HandleCastingState()
    {
        // 当玩家“按下”鼠标左键的瞬间
        if (Input.GetMouseButtonDown(0))                        
        {                                                       
            // 命令“专业工具”开始蓄力
            PowerBarController.StartCharging();                 
        }                                                       

        // 当玩家“松开”鼠标左键的瞬间
        if (Input.GetMouseButtonUp(0))                        
        {                                                  
            // 命令“专业工具”停止蓄力
            PowerBarController.StopCharging();              

            // 从“专业工具”那里获取最终的蓄力结果
            float finalPowerValue = PowerBarController.PowerBarSlider.value; 
            Debug.Log("获取到最终蓄力值: " + finalPowerValue);     

            // TODO: 在这里加入我们之前实现的“甜蜜点”判断逻辑
            // if (finalPowerValue >= sweetSpotMin && finalPowerValue <= sweetSpotMax) ...
                                                                    
            // 切换到下一个状态
            ChangeState(GameplayState.WaitingForBite);          
        }                                         
    }
}