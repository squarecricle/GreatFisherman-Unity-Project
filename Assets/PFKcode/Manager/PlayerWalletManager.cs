using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int currentGold;

    /// <summary>
    /// 增加金币
    /// </summary>
    public void AddGold(int amount)
    {
        currentGold += amount;//在目前的金币上增加amount
        // 更新UI显示
        Debug.Log($"【PlayerWallet  】增加了 {amount} 金币。当前总计: {currentGold}");
    }
}