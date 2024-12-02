using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[Serializable]
public class TextureReplacementPair
{
    public Texture2D originalTexture;
    public Material replacementMaterial;
}

[CustomEditor(typeof(MaterialSwitcherByTexture))]
public class MaterialSwitcherByTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MaterialSwitcherByTexture switcher = (MaterialSwitcherByTexture)target;

        // Add a button to trigger material replacement
        if (GUILayout.Button("Replace Materials"))
        {
            Undo.RecordObject(switcher, "Replace Materials"); // Allow undo operation in editor
            switcher.ReplaceMaterials(); // Call the ReplaceMaterials method
        }

        // Add a button to clean the array
        if (GUILayout.Button("Clean Array"))
        {
            switcher.ClearTexturePairs();
        }
    }
}

public class MaterialSwitcherByTexture : MonoBehaviour
{
    public List<TextureReplacementPair> texturePairs = new List<TextureReplacementPair>(); // List of texture pairs

    // Function to replace materials (made public)
    public void ReplaceMaterials()
    {
        // Get all MeshRenderers in the hierarchy
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        // Loop through all MeshRenderers
        foreach (MeshRenderer renderer in renderers)
        {
            // Get the materials of the renderer
            Material[] materials = renderer.sharedMaterials;

            // Loop through all materials of the renderer
            for (int i = 0; i < materials.Length; i++)
            {
                // Check each texture pair
                foreach (TextureReplacementPair pair in texturePairs)
                {
                    // Get the texture of the material
                    Texture2D materialTexture = GetTextureFromMaterial(materials[i]);

                    // Check if the texture matches the original texture
                    if (materialTexture == pair.originalTexture)
                    {
                        // Replace the material with the corresponding replacement material
                        materials[i] = pair.replacementMaterial;
                    }
                }
            }

            // Apply the modified materials to the renderer
            renderer.sharedMaterials = materials;
        }
    }

    // Function to extract the main texture from a material
    private Texture2D GetTextureFromMaterial(Material material)
    {
        if (material == null) return null;

        // Check if the material has a main texture
        if (material.mainTexture != null)
        {
            return material.mainTexture as Texture2D;
        }

        // Check if the material has a texture property
        foreach (string propertyName in material.GetTexturePropertyNames())
        {
            Texture texture = material.GetTexture(propertyName);
            if (texture != null)
            {
                return texture as Texture2D;
            }
        }

        return null;
    }

    // Function to clear the texture pairs list
    public void ClearTexturePairs()
    {
        texturePairs.Clear();
    }
}
