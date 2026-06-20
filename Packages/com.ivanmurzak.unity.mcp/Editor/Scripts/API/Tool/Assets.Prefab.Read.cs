#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
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
            "Assets_Prefab_Read",
            Title = "Read prefab content"
        )]
        [Description("Read a prefab content. Use it for get started with prefab editing. There are two options to open prefab:\n" +
                    "1. Read prefab from asset using 'prefabAssetPath'.\n" +
                    "2. Read prefab from GameObject in loaded scene using 'instanceID' of the GameObject.\n" +
                    "   The GameObject should be connected to a prefab.")]
        public string Read
        (
            [Description("'instanceID' of GameObject in a scene.")]
            int instanceID = 0,
            [Description("Prefab asset path. Should be in the format 'Assets/Path/To/Prefab.prefab'.")]
            string? prefabAssetPath = null,
            [Description("Determines the depth of the hierarchy to include. 0 - means only the target GameObject. 1 - means to include one layer below.")]
            int includeChildrenDepth = 3
        )
        => MainThread.Instance.Run(() =>
        {
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

            var prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabAssetPath);
            if (prefab == null)
                return Error.NotFoundPrefabAtPath(prefabAssetPath);

            var components = prefab.GetComponents<UnityEngine.Component>();
            var componentsPreview = components
                .Select((c, i) => Reflector.Instance.Serialize(
                    c,
                    name: $"[{i}]",
                    recursive: false,
                    logger: McpPlugin.Instance.Logger
                ))
                .ToList();

            return @$"[Success] Found Prefab at '{prefabAssetPath}'.
# Components preview:
{JsonUtils.Serialize(componentsPreview)}

# GameObject bounds:
{JsonUtils.Serialize(prefab.CalculateBounds())}

# GameObject information:
{prefab.ToMetadata(includeChildrenDepth).Print()}";
        });
    }
}