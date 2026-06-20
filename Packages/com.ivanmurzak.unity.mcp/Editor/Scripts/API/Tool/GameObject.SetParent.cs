#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor.SceneManagement;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_SetParent",
            Title = "Set parent GameObject in opened Prefab or in a Scene"
        )]
        [Description(@"Set GameObjects in opened Prefab or in a Scene by 'instanceID' (int) array.")]
        public string SetParent
        (
            GameObjectRefList gameObjectRefs,
            GameObjectRef parentGameObjectRef,
            [Description("A boolean flag indicating whether the GameObject's world position should remain unchanged when setting its parent.")]
            bool worldPositionStays = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var stringBuilder = new StringBuilder();
                int changedCount = 0;

                for (var i = 0; i < gameObjectRefs.Count; i++)
                {
                    var targetGo = GameObjectUtils.FindBy(gameObjectRefs[i], out var error);
                    if (error != null)
                    {
                        stringBuilder.AppendLine(error);
                        continue;
                    }

                    var parentGo = GameObjectUtils.FindBy(parentGameObjectRef, out error);
                    if (error != null)
                    {
                        stringBuilder.AppendLine(error);
                        continue;
                    }

                    targetGo.transform.SetParent(parentGo.transform, worldPositionStays: worldPositionStays);
                    changedCount++;

                    stringBuilder.AppendLine(@$"[Success] Set parent of {gameObjectRefs[i]} to {parentGameObjectRef}.");
                }

                if (changedCount > 0)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                return stringBuilder.ToString();
            });
        }
    }
}