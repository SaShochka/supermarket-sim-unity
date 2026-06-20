#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Editor_Selection
    {
        [McpPluginTool
        (
            "Editor_Selection_Get",
            Title = "Get current Selection value in Unity Editor"
        )]
        [Description(@"'UnityEditor.Selection'. Access to the selection in the editor.
Use it to get information about selected Assets or GameObjects in a scene.
Selection.transforms - Returns the top level selection instanceIDs, excluding Prefabs.
Selection.instanceIDs - The actual unfiltered selection from the Scene returned as instance ids instead of objects.
Selection.gameObjects - Returns the actual game object selection. Includes Prefabs, non-modifiable objects. (Read Only)
Selection.assetGUIDs - Returns the guids of the selected assets. (Read Only)
Selection.activeGameObject - Returns the active game object. (The one shown in the inspector). (Read Only)
Selection.activeInstanceID - Returns the instanceID of the actual object selection. Includes Prefabs, non-modifiable objects.
Selection.activeObject - Returns the actual object selection. Includes Prefabs, non-modifiable objects.
Selection.activeTransform - Returns the active transform. (The one shown in the inspector).")]
        public string Get()
        {
            return MainThread.Instance.Run(() => "[Success] " + SelectionPrint);
        }
    }
}