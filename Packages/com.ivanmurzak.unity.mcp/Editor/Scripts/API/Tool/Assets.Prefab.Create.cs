#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet;
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
            "Assets_Prefab_Create",
            Title = "Create prefab from a GameObject in a scene"
        )]
        [Description("Create a prefab from a GameObject in a scene. The prefab will be saved in the project assets at the specified path.")]
        public string Create
        (
            [Description("Prefab asset path. Should be in the format 'Assets/Path/To/Prefab.prefab'.")]
            string prefabAssetPath,
            [Description("'instanceID' of GameObject in a scene.")]
            int instanceID,
            [Description("If true, the prefab will replace the GameObject in the scene.")]
            bool replaceGameObjectWithPrefab = true
        )
        => MainThread.Instance.Run(() =>
        {
            if (string.IsNullOrEmpty(prefabAssetPath))
                return Error.PrefabPathIsEmpty();

            if (!prefabAssetPath.EndsWith(".prefab"))
                return Error.PrefabPathIsInvalid(prefabAssetPath);

            var go = GameObjectUtils.FindByInstanceID(instanceID);
            if (go == null)
                return Tool_GameObject.Error.NotFoundGameObjectWithInstanceID(instanceID);

            var prefabGo = replaceGameObjectWithPrefab
                ? PrefabUtility.SaveAsPrefabAsset(go, prefabAssetPath)
                : PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabAssetPath, InteractionMode.UserAction, out _);

            if (prefabGo == null)
                return Error.NotFoundPrefabAtPath(prefabAssetPath);

            EditorUtility.SetDirty(go);
            EditorApplication.RepaintHierarchyWindow();

            var result = Reflector.Instance.Serialize(
                prefabGo,
                recursive: false,
                logger: McpPlugin.Instance.Logger
            );

            return $"[Success] Prefab '{prefabAssetPath}' created from GameObject '{go.name}' (InstanceID: {instanceID}).\n" +
                   $"Prefab GameObject:\n{result}";
        });
    }
}