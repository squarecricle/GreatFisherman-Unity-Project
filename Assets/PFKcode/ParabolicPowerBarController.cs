using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间

public class ParabolicPowerBarController : MonoBehaviour//变速曲线力度条控制器
{
    [Header("UI组件关联")]
    public Slider PowerBarSlider; // 拖入你的力度条

    [Header("抛物线速度模型配置")]
    [Tooltip("力度条从0到1的总时长(秒)")]
    public float TotalDuration = 2.0f;

    [Tooltip("定义速度曲线：X轴是时间(0-1)，Y轴是进度条Value(0-1)")]
    public AnimationCurve SpeedProfileCurve;// 拖入一个抛物线形状的曲线

    // --- 私有变量 ---
    private float _timer;          // 当前的计时器
    private bool _isCharging;      // 是否正在蓄力

    void Update()
    {
        // 检测玩家输入
        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopCharging();
        }

        // 如果正在蓄力，则更新逻辑
        if (_isCharging)
        {
            UpdatePowerBar();
        }
    }

    /// <summary>
    /// 开始蓄力
    /// </summary>
    public void StartCharging()
    {
        _timer = 0f;
        _isCharging = true;
        Debug.Log("开始蓄力...");
    }

    /// <summary>
    /// 停止蓄力（方法）
    /// </summary>
    public void StopCharging()
    {
        if (!_isCharging) return;// 如果没有在蓄力，直接返回

        _isCharging = false;// 如果在蓄力则停止蓄力
        Debug.Log($"蓄力结束！最终值为: {PowerBarSlider.value}");
    }

    /// <summary>
    /// 核心：更新力度条的数值
    /// </summary>
    private void UpdatePowerBar()
    {
        // 1. 计时器按固定速度累加
        _timer += Time.deltaTime;

        // 2. 确保计时器不会超过总时长
        if (_timer >= TotalDuration)
        {
            _timer = TotalDuration;
            _isCharging = false; // 到达最大值，自动停止
            Debug.Log("蓄力已满！");
        }

        // 3. 【最关键的一步】
        //    将线性时间(_timer / TotalDuration) 映射到 AnimationCurve 上，
        //    获取一个非线性的进度条Value。
        float currentValue = SpeedProfileCurve.Evaluate(_timer / TotalDuration);//动画曲线实例.映射(归一化时间)

        // 4. 将计算出的新值赋给力度条
        PowerBarSlider.value = currentValue;
    }
}