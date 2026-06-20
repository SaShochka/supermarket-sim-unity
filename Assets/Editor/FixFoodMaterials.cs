using UnityEngine;
using UnityEditor;
using System.IO;

public class FixFoodMaterials
{
    [MenuItem("Supermarket/Fix Food Materials")]
    public static void FixMaterials()
    {
        string matPath = "Assets/Low Poly Cartoon Food and Groceries Pack/Models/Materials";
        string texPath = "Assets/Low Poly Cartoon Food and Groceries Pack/Models/Textures";

        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { matPath });
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                string texName = mat.name;
                
                // Try to find matching texture
                string texFile = $"{texPath}/{texName}.png";
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texFile);
                
                if (tex != null)
                {
                    // Assign to all possible properties
                    if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", tex);
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                    
                    EditorUtility.SetDirty(mat);
                    Debug.Log($"Fixed texture for {mat.name}");
                }
                else
                {
                    Debug.LogWarning($"Could not find texture for {mat.name} at {texFile}");
                }
            }
        }
        AssetDatabase.SaveAssets();
    }
}