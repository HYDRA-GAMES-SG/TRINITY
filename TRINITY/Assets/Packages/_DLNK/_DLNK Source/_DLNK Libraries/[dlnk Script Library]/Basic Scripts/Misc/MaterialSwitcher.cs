using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[Serializable]
public class MaterialReplacementPair
{
    public Material originalMaterial;
    public Material replacementMaterial;
}

[CustomEditor(typeof(MaterialSwitcher))]
public class MaterialSwitcherEditor : Editor
{
    private Texture2D[] originalPreviews;
    private Texture2D[] replacementPreviews;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MaterialSwitcher switcher = (MaterialSwitcher)target;

        // Display buttons horizontally
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.green;
        // Add a button to add a new element
        if (GUILayout.Button("Add Element"))
        {
            switcher.materialPairs.Add(new MaterialReplacementPair());
        }

        GUI.backgroundColor = Color.yellow;
        // Add a button to trigger material replacement
        if (GUILayout.Button("Replace Materials"))
        {
            Undo.RecordObject(switcher, "Replace Materials"); // Allow undo operation in editor
            switcher.ReplaceMaterials(); // Call the ReplaceMaterials method
        }

        GUI.backgroundColor = Color.red;
        // Add a button to clean the array
        if (GUILayout.Button("Clean Array"))
        {
            switcher.ClearMaterialPairs();
        }

        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white; // Reset background color

        GUILayout.Space(10);

        // Ensure previews are initialized
        InitializePreviews(switcher.materialPairs.Count);

        // Display material pairs with dropdown lists for original materials
        for (int i = 0; i < switcher.materialPairs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(); // Begin vertical layout

            // Display dropdown list for original material selection
            int originalIndex = GetMaterialIndex(switcher, i, switcher.materialPairs[i].originalMaterial);
            originalIndex = EditorGUILayout.Popup("Original Material", originalIndex, GetMaterialNames(switcher));
            Undo.RecordObject(switcher, "Change Original Material");
            switcher.materialPairs[i].originalMaterial = GetMaterialAtIndex(switcher, originalIndex);

            // Show original material preview below the selector
            originalPreviews[i] = GetMaterialPreview(switcher.materialPairs[i].originalMaterial);
            GUILayout.Label(originalPreviews[i], GUILayout.Width(130), GUILayout.Height(130));

            EditorGUILayout.EndVertical(); // End vertical layout

            // Show replacement material preview below the original material preview
            EditorGUILayout.BeginVertical(); // Begin vertical layout

            GUILayout.Label("Replacement Material");
            Undo.RecordObject(switcher, "Change Replacement Material");
            switcher.materialPairs[i].replacementMaterial = (Material)EditorGUILayout.ObjectField(switcher.materialPairs[i].replacementMaterial, typeof(Material), false);
            replacementPreviews[i] = GetMaterialPreview(switcher.materialPairs[i].replacementMaterial);
            GUILayout.Label(replacementPreviews[i], GUILayout.Width(130), GUILayout.Height(130));

            EditorGUILayout.EndVertical(); // End vertical layout

            EditorGUILayout.EndHorizontal();
        }
    }



    // Function to ensure previews are initialized
    private void InitializePreviews(int count)
    {
        if (originalPreviews == null || originalPreviews.Length != count)
        {
            originalPreviews = new Texture2D[count];
            replacementPreviews = new Texture2D[count];
        }
    }

    // Function to get the material index in the list of materials in the switcher
    private int GetMaterialIndex(MaterialSwitcher switcher, int pairIndex, Material material)
    {
        if (material == null)
            return 0;

        for (int i = 0; i < switcher.GetUniqueMaterials().Count; i++)
        {
            if (switcher.GetUniqueMaterials()[i] == material)
                return i;
        }

        return 0;
    }

    // Function to get the material at a specific index in the list of materials in the switcher
    private Material GetMaterialAtIndex(MaterialSwitcher switcher, int index)
    {
        if (index >= 0 && index < switcher.GetUniqueMaterials().Count)
            return switcher.GetUniqueMaterials()[index];
        else
            return null;
    }

    // Function to get an array of material names from the switcher
    private string[] GetMaterialNames(MaterialSwitcher switcher)
    {
        List<string> names = new List<string>();

        foreach (Material material in switcher.GetUniqueMaterials())
        {
            names.Add(material.name);
        }

        return names.ToArray();
    }

    // Function to get the material preview or null if the material is null
    private Texture2D GetMaterialPreview(Material material)
    {
        if (material == null)
            return null;

        return AssetPreview.GetAssetPreview(material);
    }
}

public class MaterialSwitcher : MonoBehaviour
{
    public List<MaterialReplacementPair> materialPairs = new List<MaterialReplacementPair>(); // List of material pairs

    // Function to replace materials (made public)
    public void ReplaceMaterials()
    {
        // Get all MeshRenderers in the hierarchy
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        // Record the objects for undo
        List<UnityEngine.Object> objectsToUndo = new List<UnityEngine.Object>(renderers.Length);
        objectsToUndo.Add(this); // Add the MaterialSwitcher itself to the list of objects to undo
        objectsToUndo.AddRange(renderers); // Add all MeshRenderers to the list of objects to undo
        foreach (MeshRenderer renderer in renderers)
        {
            objectsToUndo.AddRange(renderer.sharedMaterials); // Add all materials of each MeshRenderer to the list of objects to undo
        }

        Undo.RecordObjects(objectsToUndo.ToArray(), "Replace Materials"); // Allow undo operation in editor for all affected objects

        // Loop through all MeshRenderers
        foreach (MeshRenderer renderer in renderers)
        {
            // Get the materials of the renderer
            Material[] materials = renderer.sharedMaterials;

            // Loop through all materials of the renderer
            for (int i = 0; i < materials.Length; i++)
            {
                // Check each material pair
                foreach (MaterialReplacementPair pair in materialPairs)
                {
                    // Check if the material matches the original material
                    if (materials[i] == pair.originalMaterial)
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

    // Function to clear the material pairs list
    public void ClearMaterialPairs()
    {
        materialPairs.Clear();
    }

    // Function to get a list of unique materials from the children MeshRenderers
    public List<Material> GetUniqueMaterials()
    {
        List<Material> uniqueMaterials = new List<Material>();

        // Get all MeshRenderers in the hierarchy
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        // Loop through all MeshRenderers
        foreach (MeshRenderer renderer in renderers)
        {
            // Get the materials of the renderer
            Material[] materials = renderer.sharedMaterials;

            // Loop through all materials of the renderer
            foreach (Material material in materials)
            {
                if (material != null && !uniqueMaterials.Contains(material))
                {
                    uniqueMaterials.Add(material);
                }
            }
        }

        return uniqueMaterials;
    }
}
