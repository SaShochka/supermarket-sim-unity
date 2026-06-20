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
            "Scene_Load",
            Title = "Load scene"
        )]
        [Description("Load scene from the project assets.")]
        public string Load
        (
            [Description("Path to the scene file.")]
            string path,
            [Description("Load scene mode. 0 - Single, 1 - Additive.")]
            int loadSceneMode = 0
        )
        => MainThread.Instance.Run(() =>
        {
            if (string.IsNullOrEmpty(path))
                return Error.ScenePathIsEmpty();

            if (path.EndsWith(".unity") == false)
                return Error.FilePathMustEndsWithUnity();

            if (loadSceneMode < 0 || loadSceneMode > 1)
                return Error.InvalidLoadSceneMode(loadSceneMode);

            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                path,
                loadSceneMode switch
                {
                    0 => UnityEditor.SceneManagement.OpenSceneMode.Single,
                    1 => UnityEditor.SceneManagement.OpenSceneMode.Additive,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(loadSceneMode), "Invalid open scene mode.")
                });

            if (!scene.IsValid())
                return $"[Error] Failed to load scene at '{path}'.\n{LoadedScenes}";

            return $"[Success] Scene loaded at '{path}'.\n{LoadedScenes}";
        });
    }
}