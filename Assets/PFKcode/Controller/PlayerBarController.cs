using UnityEngine;

public class PlayerBarController : MonoBehaviour//玩家控制的绿条
{
    [Header("游戏参数")]
    public float MoveSpeed = 300f; // 上升速度
    public float Gravity = 800f;   // 受到的重力

    [Header("状态变量")]
    private float _verticalVelocity = 0f; // 当前的垂直速度
    private float _minY;                  // 活动范围的最小Y值
    private float _maxY;                  // 活动范围的最大Y值
    private RectTransform _rectTransform; // 自身的RectTransform组件

    // 这个初始化方法由外部（GameManager）调用，告诉绿条它的活动范围
    public void Initialize(float fishingAreaHeight, float initialY)//传入钓鱼区域的高度和初始位置
    {
        _rectTransform = GetComponent<RectTransform>();

        float halfPlayerBarHeight = _rectTransform.rect.height / 2;
        _minY = -fishingAreaHeight / 2 + halfPlayerBarHeight;
        _maxY = fishingAreaHeight / 2 - halfPlayerBarHeight;

        // 重置速度和位置
        _verticalVelocity = 0f;
        // 2. 使用传入的 initialY 参数来设置初始位置，不再写死为0
        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, Mathf.Clamp(initialY, _minY, _maxY));
    }

    // 我们不再使用Update，而是创建一个由GameManager调用的公共方法
    // 这样，GameManager就能精确控制逻辑的执行时机
    public void HandleUpdate()//被GameManager中的update调用，从而每帧更新的能力
    {
        HandleInput();
        MoveBar();
    }

    private void HandleInput()//
    {
        if (Input.GetMouseButton(0)) // 鼠标左键按住
        {
            _verticalVelocity = MoveSpeed;
        }
        else
        {
            _verticalVelocity -= Gravity * Time.deltaTime;
        }
    }

    private void MoveBar()
    {
        // 根据速度更新位置
        _rectTransform.anchoredPosition += new Vector2(0, _verticalVelocity * Time.deltaTime);

        // 限制绿条不出界
        float currentY = _rectTransform.anchoredPosition.y;
        currentY = Mathf.Clamp(currentY, _minY, _maxY);//限制绿条在活动范围内
        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, currentY);//更新绿条位置
    }
}