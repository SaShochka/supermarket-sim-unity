#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Assets
    {
        [McpPluginTool
        (
            "Assets_Refresh",
            Title = "Assets Refresh"
        )]
        [Description(@"Refreshes the AssetDatabase. Use it if any new files were added or updated in the project outside of Unity API.
Don't need to call it for Scripts manipulations.
It also triggers scripts recompilation if any changes in '.cs' files.")]
        public string Refresh() => MainThread.Instance.Run(() =>
        {
            AssetDatabase.Refresh();
            return @$"[Success] AssetDatabase refreshed. {AssetDatabase.GetAllAssetPaths().Length} assets found. Use 'Assets_Search' for more details.";
        });
    }
}