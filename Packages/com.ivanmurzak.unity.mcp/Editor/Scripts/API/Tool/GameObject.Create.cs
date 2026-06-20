#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor;
using UnityEngine;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_Create",
            Title = "Create a new GameObject in opened Prefab or in a Scene"
        )]
        [Description(@"Create a new GameObject at specific path.
if needed - provide proper 'position', 'rotation' and 'scale' to reduce amount of operations.")]
        public string Create
        (
            [Description("Name of the new GameObject.")]
            string name,
            GameObjectRef? parentGameObjectRef = null,
            [Description("Transform position of the GameObject.")]
            Vector3? position = default,
            [Description("Transform rotation of the GameObject. Euler angles in degrees.")]
            Vector3? rotation = default,
            [Description("Transform scale of the GameObject.")]
            Vector3? scale = default,
            [Description("World or Local space of transform.")]
            bool isLocalSpace = false,
            [Description("-1 - No primitive type; 0 - Cube; 1 - Sphere; 2 - Capsule; 3 - Cylinder; 4 - Plane; 5 - Quad.")]
            int primitiveType = -1
        )
        => MainThread.Instance.Run(() =>
        {
            if (string.IsNullOrEmpty(name))
                return Error.GameObjectNameIsEmpty();

            var parentGo = default(GameObject);
            if (parentGameObjectRef?.IsValid ?? false)
            {
                parentGo = GameObjectUtils.FindBy(parentGameObjectRef, out var error);
                if (error != null)
                    return error;
            }

            var go = primitiveType switch
            {
                0 => GameObject.CreatePrimitive(PrimitiveType.Cube),
                1 => GameObject.CreatePrimitive(PrimitiveType.Sphere),
                2 => GameObject.CreatePrimitive(PrimitiveType.Capsule),
                3 => GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                4 => GameObject.CreatePrimitive(PrimitiveType.Plane),
                5 => GameObject.CreatePrimitive(PrimitiveType.Quad),
                _ => new GameObject(name)
            };
            go.name = name;
            go.SetTransform(position, rotation, scale, isLocalSpace);

            if (parentGo != null)
                go.transform.SetParent(parentGo.transform, false);

            EditorUtility.SetDirty(go);
            EditorApplication.RepaintHierarchyWindow();

            return $"[Success] Created GameObject.\n{go.Print()}";
        });
    }
}