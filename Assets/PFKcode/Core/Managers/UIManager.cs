using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责统一管理 UI 面板的显示/隐藏，提供名称到实例的映射。
/// </summary>
public class UIManager : MonoBehaviour
{
    [System.Serializable]
    private struct PanelEntry
    {
        [SerializeField] public string panelName;
        [SerializeField] public GameObject panelRoot;
    }

    public static UIManager Instance { get; private set; }

    [SerializeField] private List<PanelEntry> registeredPanels = new();

    private readonly Dictionary<string, GameObject> _panelLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildLookup();
    }

    private void BuildLookup()
    {
        _panelLookup.Clear();
        foreach (var entry in registeredPanels)
        {
            if (string.IsNullOrEmpty(entry.panelName) || entry.panelRoot == null)
            {
                continue;
            }

            if (!_panelLookup.ContainsKey(entry.panelName))
            {
                _panelLookup.Add(entry.panelName, entry.panelRoot);
            }
            else
            {
                Debug.LogWarning($"[UIManager] 面板名称 {entry.panelName} 重复注册，仅会保留第一个实例。");
            }
        }
    }

    private void OnValidate()
    {
        BuildLookup();
    }

    public void ShowPanel(string panelName, bool hideOthers = true)
    {
        if (!_panelLookup.TryGetValue(panelName, out var panel))
        {
            Debug.LogError($"[UIManager] 未注册名为 {panelName} 的面板。");
            return;
        }

        if (hideOthers)
        {
            foreach (var kvp in _panelLookup)
            {
                kvp.Value.SetActive(kvp.Key == panelName);
            }
        }
        else
        {
            panel.SetActive(true);
        }
    }

    public void HidePanel(string panelName)
    {
        if (_panelLookup.TryGetValue(panelName, out var panel))
        {
            panel.SetActive(false);
        }
    }

    public GameObject GetPanel(string panelName)
    {
        _panelLookup.TryGetValue(panelName, out var panel);
        return panel;
    }

    /// <summary>
    /// 允许在运行时动态注册新的面板（例如通过预制件实例化后交由 UIManager 管理）。
    /// </summary>
    public void RegisterPanel(string panelName, GameObject panelRoot, bool overwrite = false)
    {
        if (string.IsNullOrEmpty(panelName) || panelRoot == null)
        {
            Debug.LogError("[UIManager] RegisterPanel 调用参数无效。");
            return;
        }

        if (_panelLookup.ContainsKey(panelName))
        {
            if (!overwrite)
            {
                Debug.LogWarning($"[UIManager] 面板 {panelName} 已存在，若需覆盖请将 overwrite 设为 true。");
                return;
            }

            _panelLookup[panelName] = panelRoot;
        }
        else
        {
            _panelLookup.Add(panelName, panelRoot);
        }
    }
}
