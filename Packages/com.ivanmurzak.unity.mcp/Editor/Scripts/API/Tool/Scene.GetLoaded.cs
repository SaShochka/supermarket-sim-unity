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
            "Scene_GetLoaded",
            Title = "Get list of currently loaded scenes"
        )]
        [Description("Returns the list of currently loaded scenes.")]
        public string GetLoaded() => MainThread.Instance.Run(() =>
        {
            return $"[Success] " + LoadedScenes;
        });
    }
}