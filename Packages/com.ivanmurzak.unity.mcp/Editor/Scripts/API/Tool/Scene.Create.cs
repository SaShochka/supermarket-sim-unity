#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Scene
    {
        [McpPluginTool
        (
            "Scene_Create",
            Title = "Create new scene"
        )]
        [Description("Create new scene in the project assets.")]
        public string Create
        (
            [Description("Path to the scene file.")]
            string path
        )
        => MainThread.Instance.Run(() =>
        {
            if (string.IsNullOrEmpty(path))
                return Error.ScenePathIsEmpty();

            if (path.EndsWith(".unity") == false)
                return Error.FilePathMustEndsWithUnity();

            // Create a new empty scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // Save the scene asset at the specified path
            bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
            if (!saved)
                return $"[Error] Failed to save scene at '{path}'.\n{LoadedScenes}";

            return $"[Success] Scene created at '{path}'.\n{LoadedScenes}";
        });
    }
}