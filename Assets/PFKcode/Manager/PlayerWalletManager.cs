using UnityEngine;

public class PlayerWalletManager : MonoBehaviour
{
    public int currentGold;

    /// <summary>
    /// 增加金币
    /// </summary>
    public void AddGold(int amount)
    {
        // TODO: 阶段二实现
        // currentGold += amount;
        // 更新UI显示
    Debug.Log($"【PlayerWalletManager】增加了 {amount} 金币。当前总计: {currentGold + amount}");
    }
}