#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Scene
    {
        [McpPluginTool
        (
            "Scene_GetHierarchy",
            Title = "Get Scene Hierarchy"
        )]
        [Description("This tool retrieves the list of root GameObjects in the specified scene.")]
        public string GetHierarchyRoot
        (
            [Description("Determines the depth of the hierarchy to include.")]
            int includeChildrenDepth = 3,
            [Description("Name of the loaded scene. If empty string, the active scene will be used.")]
            string? loadedSceneName = null
        )
        => MainThread.Instance.Run(() =>
        {
            var scene = string.IsNullOrEmpty(loadedSceneName)
                ? UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                : UnityEngine.SceneManagement.SceneManager.GetSceneByName(loadedSceneName);

            if (!scene.IsValid())
                return Error.NotFoundSceneWithName(loadedSceneName);

            return scene.ToMetadata(includeChildrenDepth: includeChildrenDepth).Print();
        });
    }
}