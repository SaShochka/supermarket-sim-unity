#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.IO;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Script
    {
        [McpPluginTool
        (
            "Script_CreateOrUpdate",
            Title = "Create or Update Script"
        )]
        [Description("Creates or updates a script file with the provided content. Does AssetDatabase.Refresh() at the end.")]
        public string UpdateOrCreate
        (
            [Description("The path to the file. Sample: \"Assets/Scripts/MyScript.cs\".")]
            string filePath,
            [Description("C# code - content of the file.")]
            string content
        )
        {
            if (string.IsNullOrEmpty(filePath))
                return Error.ScriptPathIsEmpty();

            if (!filePath.EndsWith(".cs"))
                return Error.FilePathMustEndsWithCs();

            if (!ScriptUtils.IsValidCSharpSyntax(content, out var errors))
                return $"[Error] Invalid C# syntax:\n{string.Join("\n", errors)}";

            var dirPath = Path.GetDirectoryName(filePath)!;
            if (Directory.Exists(dirPath) == false)
                Directory.CreateDirectory(dirPath);

            File.WriteAllText(filePath, content);

            return MainThread.Instance.Run(() =>
            {
                AssetDatabase.Refresh();
                return $"[Success] Script created or updated at: {filePath}";
            });
        }
    }
}