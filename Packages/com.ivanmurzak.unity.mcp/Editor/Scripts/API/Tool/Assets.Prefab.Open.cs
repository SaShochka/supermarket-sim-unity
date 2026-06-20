#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Assets_Prefab
    {
        [McpPluginTool
        (
            "Assets_Prefab_Open",
            Title = "Open prefab"
        )]
        [Description("Open a prefab. There are two options to open prefab:\n" +
                    "1. Open prefab from asset using 'prefabAssetPath'.\n" +
                    "2. Open prefab from GameObject in loaded scene using 'instanceID' of the GameObject.\n" +
                    "   The GameObject should be connected to a prefab.\n\n" +
                    "Note: Please 'Close' the prefab later to exit prefab editing mode.")]
        public string Open
        (
            [Description("'instanceID' of GameObject in a scene.")]
            int instanceID = 0,
            [Description("Prefab asset path. Should be in the format 'Assets/Path/To/Prefab.prefab'.")]
            string? prefabAssetPath = null
        )
        => MainThread.Instance.Run(() =>
        {
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            // if (prefabStage != null)
            //     return Error.PrefabStageIsAlreadyOpened();

            if (string.IsNullOrEmpty(prefabAssetPath) && instanceID != 0)
            {
                // Find prefab from GameObject in loaded scene
                var go = GameObjectUtils.FindByInstanceID(instanceID);
                if (go == null)
                    return Tool_GameObject.Error.NotFoundGameObjectWithInstanceID(instanceID);

                prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            }

            if (string.IsNullOrEmpty(prefabAssetPath))
                return Error.PrefabPathIsEmpty();

            var goInstance = instanceID != 0
                ? GameObjectUtils.FindByInstanceID(instanceID)
                : null;

            prefabStage = goInstance != null
                ? UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(prefabAssetPath, goInstance)
                : UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(prefabAssetPath);

            if (prefabStage == null)
                return Error.PrefabStageIsNotOpened();

            return @$"[Success] Prefab '{prefabStage.assetPath}' opened. Use 'Assets_Prefab_Close' to close it.
# Prefab information:
{prefabStage.prefabContentsRoot.ToMetadata().Print()}";
        });
    }
}