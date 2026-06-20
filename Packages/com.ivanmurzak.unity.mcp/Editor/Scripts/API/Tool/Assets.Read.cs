#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Assets
    {
        [McpPluginTool
        (
            "Assets_Read",
            Title = "Read asset file content"
        )]
        [Description(@"Read file asset in the project.")]
        public string Read
        (
            [Description("Path to the asset. See 'Assets_Search' for more details. Starts with 'Assets/'. Priority: 1. (Recommended)")]
            string? assetPath = null,
            [Description("GUID of the asset. Priority: 2.")]
            string? assetGuid = null
        )
        => MainThread.Instance.Run(() =>
        {
            if (string.IsNullOrEmpty(assetPath) && string.IsNullOrEmpty(assetGuid))
                return Error.NeitherProvided_AssetPath_AssetGuid();

            if (string.IsNullOrEmpty(assetPath))
                assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

            if (string.IsNullOrEmpty(assetGuid))
                assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
                return Error.NotFoundAsset(assetPath, assetGuid);

            var serialized = Reflector.Instance.Serialize(
                asset,
                name: asset.name,
                recursive: true,
                logger: McpPlugin.Instance.Logger
            );
            var json = JsonUtils.Serialize(serialized);

            return $"[Success] Loaded asset at path '{assetPath}'.\n{json}";

            //             var instanceID = asset.GetInstanceID();
            //             return @$"[Success] Loaded asset.
            // # Asset path: {assetPath}
            // # Asset GUID: {assetGuid}
            // # Asset instanceID: {instanceID}";
        });
    }
}