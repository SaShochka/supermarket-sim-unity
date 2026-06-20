#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Assets
    {
        [McpPluginTool
        (
            "Assets_CreateFolders",
            Title = "Assets Create Folders"
        )]
        [Description(@"Create folders at specific locations in the project.
Use it to organize scripts and assets in the project. Does AssetDatabase.Refresh() at the end.")]
        public string CreateFolders
        (
            [Description("The paths for the folders to create.")]
            string[] paths
        )
        => MainThread.Instance.Run(() =>
        {
            if (paths.Length == 0)
                return Error.SourcePathsArrayIsEmpty();

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrEmpty(paths[i]))
                {
                    stringBuilder.AppendLine(Error.SourcePathIsEmpty());
                    continue;
                }
                try
                {
                    Directory.CreateDirectory(paths[i]);
                    stringBuilder.AppendLine($"[Success] Created folder at {paths[i]}.");
                }
                catch (Exception e)
                {
                    stringBuilder.AppendLine($"[Error] Failed to create folder at {paths[i]}: {e.Message}");
                }
            }

            AssetDatabase.Refresh();
            return stringBuilder.ToString();
        });
    }
}