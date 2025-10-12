using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

/// <summary>
/// 将指定文件夹内的所有 C# 脚本合并到一个 TXT 文件中。
/// </summary>
public class ScriptMerger
{
    // 在Unity顶部菜单栏创建一个名为 "Tools" 的菜单，内有一个 "合并脚本到单个TXT" 的选项
    [MenuItem("Tools/Merge All C# Scripts to ONE TXT")]
    private static void MergeScriptsToSingleTxt()
    {
        // 1. 让用户选择包含 C# 脚本的源文件夹
        string sourcePath = EditorUtility.OpenFolderPanel("选择要合并的脚本源文件夹", "Assets", "");
        if (string.IsNullOrEmpty(sourcePath))
        {
            Debug.Log("操作已取消：未选择源文件夹。");
            return;
        }

        // 2. 让用户选择保存合并后 .txt 文件的路径和文件名
        string destinationPath = EditorUtility.SaveFilePanel("选择保存合并后 TXT 的路径", Application.dataPath, "Merged_Scripts", "txt");
        if (string.IsNullOrEmpty(destinationPath))
        {
            Debug.Log("操作已取消：未选择保存路径。");
            return;
        }
        
        Debug.Log($"开始合并... 源: {sourcePath} -> 目标文件: {destinationPath}");

        try
        {
            // 3. 获取源文件夹下所有的 .cs 文件 (包括子文件夹)
            string[] scriptFiles = Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories);
            
            if (scriptFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "在所选文件夹中没有找到任何 C# (.cs) 文件。", "好的");
                return;
            }

            // 4. 使用 StringBuilder 高效地拼接所有文件内容
            StringBuilder mergedContent = new StringBuilder();
            
            // --- 新增部分：在文件开头生成脚本索引 ---
            mergedContent.AppendLine("### Merged Scripts Index ###");
            mergedContent.AppendLine("```"); // 使用代码块以获得更好的格式
            foreach (string sourceFile in scriptFiles)
            {
                // 只显示文件名
                mergedContent.AppendLine($"└─ {Path.GetFileName(sourceFile)}");
            }
            mergedContent.AppendLine("```");
            mergedContent.AppendLine();
            mergedContent.AppendLine("---"); // 添加一个分隔线
            mergedContent.AppendLine();
            // --- 索引生成结束 ---

            // 5. 遍历并拼接每个文件的具体内容
            foreach (string sourceFile in scriptFiles)
            {
                string fileName = Path.GetFileName(sourceFile);
                string fileContent = File.ReadAllText(sourceFile);

                // --- 拼接格式 ---
                // 保留清晰的文件内容分隔符
                mergedContent.AppendLine($"// ---------- SCRIPT START: {fileName} ---------- //");
                mergedContent.AppendLine();
                mergedContent.Append(fileContent);
                mergedContent.AppendLine(); 
                mergedContent.AppendLine($"// ----------- SCRIPT END: {fileName} ----------- //");
                mergedContent.AppendLine();
                mergedContent.AppendLine();
            }
            
            // 6. 将拼接好的所有内容一次性写入目标文件
            File.WriteAllText(destinationPath, mergedContent.ToString());
            
            // 7. 显示成功信息
            int mergedCount = scriptFiles.Length;
            EditorUtility.DisplayDialog("合并成功", $"成功将 {mergedCount} 个 C# 脚本文件合并为一个 .txt 文件！\n\n文件已保存至：\n{destinationPath}", "太棒了！");
            Debug.Log($"合并完成！总共合并了 {mergedCount} 个文件。");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("合并失败", $"发生了一个错误: {e.Message}", "关闭");
            Debug.LogError($"合并脚本时发生错误: {e}");
        }
    }
}