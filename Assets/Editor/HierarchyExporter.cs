using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Linq; // 引入 Linq 命名空间
using UnityEngine.SceneManagement;

/// <summary>
/// Unity Hierarchy 导出器，可将场景层级结构导出为 Markdown 文件。
/// </summary>
public class HierarchyExporter
{
    // 定义菜单项，路径为 "工具/导出Hierarchy到Markdown"
    [MenuItem("Tools/Export Hierarchy to Markdown")]
    private static void ExportHierarchyToMarkdown()
    {
        // 1. 获取当前活动场景
        Scene currentScene = SceneManager.GetActiveScene();
        if (!currentScene.IsValid())
        {
            EditorUtility.DisplayDialog("错误", "没有有效的活动场景可供导出。", "确定");
            return;
        }

        // 2. 弹出文件保存对话框
        // 参数：标题, 默认目录, 默认文件名, 文件扩展名
        string path = EditorUtility.SaveFilePanel(
            "保存Hierarchy为Markdown文件",
            "Assets", // 默认打开的文件夹
            $"{currentScene.name}_Hierarchy.md", // 默认文件名
            "md"
        );

        // 如果用户取消了保存（路径为空），则直接返回
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // 3. 构建文本内容
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"# Scene Hierarchy: {currentScene.name}");
        sb.AppendLine();
        sb.AppendLine("```"); // Markdown 代码块开始，以保持缩进和等宽字体

        // 4. 获取场景中所有的根游戏对象
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        // 5. 遍历所有根对象，并递归处理它们的子对象
        foreach (GameObject root in rootObjects)
        {
            ProcessTransform(root.transform, 0, sb);
        }

        sb.AppendLine("```"); // Markdown 代码块结束

        // 6. 将内容写入文件
        try
        {
            File.WriteAllText(path, sb.ToString());
            // 在Unity控制台打印成功信息，并高亮显示生成的文件
            Debug.Log($"Hierarchy 成功导出到: <a href=\"{path}\">{path}</a>");
            AssetDatabase.Refresh(); // 刷新资源数据库，以便在Project窗口中看到新文件
        }
        catch (System.Exception e)
        {
            Debug.LogError($"文件写入失败: {e.Message}");
            EditorUtility.DisplayDialog("导出失败", $"无法写入文件到路径: {path}\n\n错误信息: {e.Message}", "确定");
        }
    }

    /// <summary>
    /// 递归处理每个Transform，构建层级字符串
    /// </summary>
    /// <param name="transform">当前处理的Transform</param>
    /// <param name="depth">当前层级深度</param>
    /// <param name="sb">用于构建字符串的StringBuilder</param>
    private static void ProcessTransform(Transform transform, int depth, StringBuilder sb)
    {
        // 根据深度添加缩进
        // "  " (两个空格) 代表一级缩进
        // "└─ " 是一个装饰，让结构更清晰
        string indent = new string(' ', depth * 2);
        string prefix = depth > 0 ? "└─ " : "";

        // 获取对象名称
        string objectName = transform.gameObject.name;

        // --- 新增代码开始 ---
        // 获取对象上所有的 MonoBehaviour 脚本组件
        var components = transform.gameObject.GetComponents<MonoBehaviour>();
        string componentsString = "";
        if (components.Length > 0)
        {
            // 使用 Linq 的 Select 方法获取所有脚本的类型名称，然后用 ", " 连接起来
            string componentNames = string.Join(", ", components.Select(c => c.GetType().Name));
            componentsString = $" [{componentNames}]"; // 格式化为 [脚本1, 脚本2]
        }
        // --- 新增代码结束 ---

        // 检查对象是否在Hierarchy中处于非激活状态
        string status = transform.gameObject.activeInHierarchy ? "" : " (Inactive)";

        // 将格式化后的行添加到StringBuilder，加入了脚本信息
        sb.AppendLine($"{indent}{prefix}{objectName}{componentsString}{status}");

        // 递归处理所有子对象
        foreach (Transform child in transform)
        {
            // 子对象的深度+1
            ProcessTransform(child, depth + 1, sb);
        }
    }
}