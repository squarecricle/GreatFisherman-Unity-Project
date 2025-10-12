using System;
using UnityEngine;

/// <summary>
/// 统一管理玩家的长期数据（金币、等级等），未来可扩展为存档系统。
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("基础属性")]
    [SerializeField] private int currentGold;

    public event Action<int> GoldChanged;

    public int CurrentGold => currentGold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentGold += amount;
        GoldChanged?.Invoke(currentGold);
        Debug.Log($"[PlayerDataManager] 玩家金币增加 {amount}，当前总计 {currentGold}。");
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (currentGold < amount)
        {
            return false;
        }

        currentGold -= amount;
        GoldChanged?.Invoke(currentGold);
        Debug.Log($"[PlayerDataManager] 玩家消费 {amount} 金币，剩余 {currentGold}。");
        return true;
    }
}
