#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS0618
using System.ComponentModel;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Editor_Selection
    {
        [McpPluginTool
        (
            "Editor_Selection_Set",
            Title = "Set Selection in Unity Editor"
        )]
        [Description(@"'UnityEditor.Selection'. Access to the selection in the editor.
Use it to select Assets or GameObjects in a scene. Set empty array to clear selection.
Selection.instanceIDs - The actual unfiltered selection from the Scene returned as instance ids.
Selection.activeInstanceID -  The 'instanceID' of the actual object selection. Includes Prefabs, non-modifiable objects.")]
        public string Set
        (
            [Description("The 'instanceID' array of the target GameObjects.")]
            int[]? instanceIDs = null,
            [Description("The 'instanceID' of the target Object.")]
            int activeInstanceID = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Selection.instanceIDs = instanceIDs ?? new int[0];
                Selection.activeInstanceID = activeInstanceID;

                return "[Success] " + SelectionPrint;
            });
        }
    }
}