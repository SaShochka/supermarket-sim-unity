#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS0618
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_Duplicate",
            Title = "Duplicate GameObjects"
        )]
        [Description(@"Duplicate GameObjects in opened Prefab or in a Scene.")]
        public string Duplicate
        (
            GameObjectRefList gameObjectRefs
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                var gos = new List<GameObject>(gameObjectRefs.Count);

                for (int i = 0; i < gameObjectRefs.Count; i++)
                {
                    var gameObjectRef = gameObjectRefs[i];
                    var go = GameObjectUtils.FindBy(gameObjectRefs[i], out var error);
                    if (error != null)
                        return error;

                    gos.Add(go);
                }
                Selection.instanceIDs = gos
                    .Select(go => go.GetInstanceID())
                    .ToArray();

                Unsupported.DuplicateGameObjectsUsingPasteboard();

                var modifiedScenes = Selection.gameObjects
                    .Select(go => go.scene)
                    .Distinct()
                    .ToList();

                foreach (var scene in modifiedScenes)
                    EditorSceneManager.MarkSceneDirty(scene);

                var location = prefabStage != null ? "Prefab" : "Scene";
                return @$"[Success] Duplicated {gos.Count} GameObjects in opened {location} by 'instanceID' (int) array.
Duplicated instanceIDs:
{string.Join(", ", Selection.instanceIDs)}";
            });
        }
    }
}