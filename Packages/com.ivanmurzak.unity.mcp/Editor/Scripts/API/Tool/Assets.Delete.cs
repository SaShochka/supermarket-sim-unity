#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Generic;
using System.ComponentModel;
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
            "Assets_Delete",
            Title = "Assets Delete"
        )]
        [Description(@"Delete the assets at paths from the project. Does AssetDatabase.Refresh() at the end.")]
        public string Delete
        (
            [Description("The paths of the assets")]
            string[] paths
        )
        => MainThread.Instance.Run(() =>
        {
            if (paths.Length == 0)
                return Error.SourcePathsArrayIsEmpty();

            var outFailedPaths = new List<string>();
            var success = AssetDatabase.DeleteAssets(paths, outFailedPaths);
            if (!success)
            {
                var stringBuilder = new StringBuilder();
                foreach (var failedPath in outFailedPaths)
                    stringBuilder.AppendLine($"[Error] Failed to delete asset at {failedPath}.");
                return stringBuilder.ToString();
            }

            AssetDatabase.Refresh();
            return "[Success] Deleted assets at paths:\n" + string.Join("\n", paths);
        });
    }
}