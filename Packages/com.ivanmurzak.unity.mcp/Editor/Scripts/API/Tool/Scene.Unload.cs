#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Scene
    {
        [McpPluginTool
        (
            "Scene_Unload",
            Title = "Unload scene"
        )]
        [Description("Destroys all GameObjects associated with the given Scene and removes the Scene from the SceneManager.")]
        public Task<string> Unload
        (
            [Description("Name of the loaded scene.")]
            string name
        )
        => MainThread.Instance.Run(async () =>
        {
            if (string.IsNullOrEmpty(name))
                return Error.SceneNameIsEmpty();

            var scene = SceneUtils.GetAllLoadedScenes()
                .FirstOrDefault(scene => scene.name == name);

            if (!scene.IsValid())
                return Error.NotFoundSceneWithName(name);

            var asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);

            while (!asyncOperation.isDone)
                await Task.Yield();

            return $"[Success] Scene '{name}' unloaded.\n{LoadedScenes}";
        });
    }
}