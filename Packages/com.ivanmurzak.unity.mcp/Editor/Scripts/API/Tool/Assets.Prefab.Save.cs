#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Assets_Prefab
    {
        [McpPluginTool
        (
            "Assets_Prefab_Save",
            Title = "Save prefab"
        )]
        [Description("Save a prefab. Use it when you are in prefab editing mode in Unity Editor.")]
        public string Save() => MainThread.Instance.Run(() =>
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
                return Error.PrefabStageIsNotOpened();

            var prefabGo = prefabStage.prefabContentsRoot;
            if (prefabGo == null)
                return Error.PrefabStageIsNotOpened();

            var assetPath = prefabStage.assetPath;
            var goName = prefabGo.name;

            PrefabUtility.SaveAsPrefabAsset(prefabGo, assetPath);

            return @$"[Success] Prefab at asset path '{assetPath}' saved. " +
                   $"Prefab with GameObject.name '{goName}'.";
        });
    }
}