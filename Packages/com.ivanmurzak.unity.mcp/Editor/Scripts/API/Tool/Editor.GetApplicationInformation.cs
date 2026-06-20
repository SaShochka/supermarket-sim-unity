#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Editor
    {
        [McpPluginTool
        (
            "Editor_GetApplicationInformation",
            Title = "Get Unity Editor application information"
        )]
        [Description(@"Returns list of available information about 'UnityEditor.EditorApplication'.
Use it to get information about the current state of the Unity Editor application. Such as: playmode, paused state, compilation state, etc.
EditorApplication.isPlaying - Whether the Editor is in Play mode.
EditorApplication.isPaused - Whether the Editor is paused.
EditorApplication.isCompiling - Is editor currently compiling scripts? (Read Only)
EditorApplication.isPlayingOrWillChangePlaymode - Editor application state which is true only when the Editor is currently in or about to enter Play mode. (Read Only)
EditorApplication.isUpdating - True if the Editor is currently refreshing the AssetDatabase. (Read Only)
EditorApplication.applicationContentsPath - Path to the Unity editor contents folder. (Read Only)
EditorApplication.applicationPath - Gets the path to the Unity Editor application. (Read Only)
EditorApplication.timeSinceStartup - The time since the editor was started. (Read Only)")]
        public string GetApplicationInformation()
        {
            return MainThread.Instance.Run(() => "[Success] " + EditorStats);
        }
    }
}