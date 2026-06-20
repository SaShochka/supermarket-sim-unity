#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS0618
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_DestroyComponents",
            Title = "Destroy Components from a GameObject in opened Prefab or in a Scene"
        )]
        [Description("Destroy one or many components from target GameObject.")]
        public string DestroyComponents
        (
            GameObjectRef gameObjectRef,
            ComponentRefList destroyComponentRefs
        )
        => MainThread.Instance.Run(() =>
        {
            var go = GameObjectUtils.FindBy(gameObjectRef, out var error);
            if (error != null)
                return error;

            var destroyCounter = 0;
            var stringBuilder = new StringBuilder();

            var allComponents = go.GetComponents<UnityEngine.Component>();
            foreach (var component in allComponents)
            {
                if (destroyComponentRefs.Any(cr => cr.Matches(component)))
                {
                    UnityEngine.Object.DestroyImmediate(component);
                    destroyCounter++;
                    stringBuilder.AppendLine($"[Success] Destroyed component instanceID='{component.GetInstanceID()}', type='{component.GetType().FullName}'.");
                }
            }

            if (destroyCounter == 0)
                return Error.NotFoundComponents(destroyComponentRefs, allComponents);

            return $"[Success] Destroyed {destroyCounter} components from GameObject.\n{stringBuilder.ToString()}";
        });
    }
}