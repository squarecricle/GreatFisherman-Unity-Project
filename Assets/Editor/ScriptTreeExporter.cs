using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq; // 用于方便地获取最后一个元素

/// <summary>
/// 将项目中 Assets 文件夹下的所有 C# 脚本结构导出为 Markdown 文件。
/// </summary>
public class ScriptTreeExporter
{
    // 定义菜单项路径 "工具/导出脚本结构树到Markdown"
    [MenuItem("Tools/Export Script Tree to Markdown")]
    private static void ExportScriptTree()
    {
        // 1. 弹出文件保存对话框，让用户选择保存路径
        string path = EditorUtility.SaveFilePanel(
            "保存脚本结构树为Markdown",
            "Assets",
            "Project_Scripts_Tree.md",
            "md");

        // 如果用户取消操作，路径为空，则直接返回
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // 2. 准备开始构建字符串
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# 项目脚本结构树");
        sb.AppendLine();
        sb.AppendLine("```"); // Markdown代码块开始

        // 3. 从 Assets 根目录开始递归遍历
        // Application.dataPath 指向项目 Assets 文件夹的绝对路径
        sb.AppendLine("Assets");
        BuildDirectoryTree(Application.dataPath, "", sb);

        sb.AppendLine("```"); // Markdown代码块结束

        // 4. 将内容写入文件
        try
        {
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"脚本结构树成功导出到: <a href=\"{path}\">{path}</a>");
            AssetDatabase.Refresh(); // 刷新编辑器，以便能看到新文件
        }
        catch (System.Exception e)
        {
            Debug.LogError($"文件写入失败: {e.Message}");
            EditorUtility.DisplayDialog("导出失败", $"无法写入文件到路径: {path}\n\n错误信息: {e.Message}", "确定");
        }
    }

    /// <summary>
    /// 递归函数，用于构建目录和文件的文本树
    /// </summary>
    /// <param name="directoryPath">当前要处理的目录路径</param>
    /// <param name="prefix">用于绘制树状结构的前缀字符串</param>
    /// <param name="sb">字符串构建器</param>
    private static void BuildDirectoryTree(string directoryPath, string prefix, StringBuilder sb)
    {
        // --- 1. 获取当前目录下的所有子目录 ---
        // 过滤掉我们不关心的目录，例如版本控制或库文件
        string[] subDirectories = Directory.GetDirectories(directoryPath)
            .Where(d => !Path.GetFileName(d).StartsWith(".") && !Path.GetFileName(d).StartsWith("Packages")).ToArray();
        
        // --- 2. 获取当前目录下的所有 .cs 脚本文件 ---
        string[] scriptFiles = Directory.GetFiles(directoryPath, "*.cs");

        // 合并目录和文件，统一处理，目录在前，文件在后
        var entries = subDirectories.Select(d => new { Path = d, IsDirectory = true })
            .Concat(scriptFiles.Select(f => new { Path = f, IsDirectory = false }))
            .ToList();

        // --- 3. 遍历所有条目（目录和文件） ---
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            bool isLast = (i == entries.Count - 1);

            // a. 构建当前行的前缀
            string linePrefix = prefix + (isLast ? "└─ " : "├─ ");
            sb.AppendLine(linePrefix + Path.GetFileName(entry.Path));

            // b. 如果是目录，则递归进入下一层
            if (entry.IsDirectory)
            {
                // 计算下一层递归的前缀
                string nextPrefix = prefix + (isLast ? "   " : "│  ");
                BuildDirectoryTree(entry.Path, nextPrefix, sb);
            }
        }
    }
}