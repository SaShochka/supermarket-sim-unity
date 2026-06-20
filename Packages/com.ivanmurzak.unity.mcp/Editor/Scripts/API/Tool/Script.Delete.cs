#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.IO;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Script
    {
        [McpPluginTool
        (
            "Script_Delete",
            Title = "Delete Script content"
        )]
        [Description("Delete the script file. Does AssetDatabase.Refresh() at the end.")]
        public string Delete
        (
            [Description("The path to the file. Sample: \"Assets/Scripts/MyScript.cs\".")]
            string filePath
        )
        {
            if (string.IsNullOrEmpty(filePath))
                return Error.ScriptPathIsEmpty();

            if (!filePath.EndsWith(".cs"))
                return Error.FilePathMustEndsWithCs();

            if (File.Exists(filePath) == false)
                return Error.ScriptFileNotFound(filePath);

            File.Delete(filePath);
            if (File.Exists(filePath + ".meta"))
                File.Delete(filePath + ".meta");

            return MainThread.Instance.Run(() =>
            {
                AssetDatabase.Refresh();
                return $"[Success] Script deleted: {filePath}";
            });
        }
    }
}