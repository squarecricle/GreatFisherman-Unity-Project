using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptConverter
{
    [MenuItem("Tools/Convert All C# Scripts to TXT")]
    private static void ConvertScriptsToTxt()
    {
        // 1. 让用户选择包含 C# 脚本的源文件夹
        string sourcePath = EditorUtility.OpenFolderPanel("选择要转换的脚本源文件夹", "Assets", "");
        if (string.IsNullOrEmpty(sourcePath))
        {
            Debug.Log("操作已取消：未选择源文件夹。");
            return;
        }

        // 2. 让用户选择保存 .txt 文件的目标文件夹
        string destinationPath = EditorUtility.OpenFolderPanel("选择保存 TXT 文件的目标文件夹", Application.dataPath, "");
        if (string.IsNullOrEmpty(destinationPath))
        {
            Debug.Log("操作已取消：未选择目标文件夹。");
            return;
        }
        
        Debug.Log($"开始转换... 源: {sourcePath} -> 目标: {destinationPath}");

        try
        {
            // 3. 获取源文件夹下所有的 .cs 文件 (包括子文件夹)
            string[] scriptFiles = Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories);
            
            if (scriptFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "在所选文件夹中没有找到任何 C# (.cs) 文件。", "好的");
                return;
            }

            int convertedCount = 0;
            foreach (string sourceFile in scriptFiles)
            {
                // 4. 构建目标文件路径，并保持目录结构
                // 创建相对于源文件夹的路径
                string relativePath = sourceFile.Substring(sourcePath.Length + 1);
                string destinationFile = Path.Combine(destinationPath, relativePath);

                // 将文件扩展名从 .cs 更改为 .txt
                destinationFile = Path.ChangeExtension(destinationFile, ".txt");

                // 确保目标目录存在
                string directoryName = Path.GetDirectoryName(destinationFile);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                // 5. 复制和重命名文件
                File.Copy(sourceFile, destinationFile, true); // true 表示如果文件已存在则覆盖
                convertedCount++;
            }
            
            // 6. 显示成功信息
            EditorUtility.DisplayDialog("转换成功", $"成功将 {convertedCount} 个 C# 脚本文件转换为 .txt 文件！\n\n文件已保存至：\n{destinationPath}", "太棒了！");
            Debug.Log($"转换完成！总共转换了 {convertedCount} 个文件。");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("转换失败", $"发生了一个错误: {e.Message}", "关闭");
            Debug.LogError($"转换脚本时发生错误: {e}");
        }
    }
}