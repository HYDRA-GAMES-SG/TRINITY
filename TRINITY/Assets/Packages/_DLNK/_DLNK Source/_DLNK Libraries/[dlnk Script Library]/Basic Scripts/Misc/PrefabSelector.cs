using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabSelector : MonoBehaviour
{
    [System.Serializable]
    public class PrefabVariant
    {
        public List<GameObject> variants = new List<GameObject>();
    }

    public List<PrefabVariant> prefabVariants = new List<PrefabVariant>();
    private GameObject currentPrefab;

    // Editor variables
    [HideInInspector] public int selectedPrefabIndex = 0;
    [HideInInspector] public int selectedVariantIndex = 0;
    public int elementsPerPage = 100; // Variable to determine how many elements to display per page

    void Start()
    {
        if (prefabVariants.Count > 0)
        {
            SwitchPrefab(selectedPrefabIndex);
        }
    }

    public void SwitchPrefab(int index)
    {
        // Destroy all existing child objects
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        if (index >= 0 && index < prefabVariants.Count)
        {
            PrefabVariant selectedVariant = prefabVariants[index];
            if (selectedVariant != null && selectedVariantIndex < selectedVariant.variants.Count)
            {
                GameObject prefab = selectedVariant.variants[selectedVariantIndex];
                if (prefab != null)
                {
                    currentPrefab = Instantiate(prefab, transform);
                    currentPrefab.transform.localPosition = Vector3.zero; // Set local position to zero
                }
                else
                {
                    Debug.LogError("Prefab is null in the selected variant.");
                }
            }
            else
            {
                Debug.LogError("Selected variant index is out of range for the prefab variant.");
            }
        }
        else
        {
            Debug.LogError("Selected prefab index is out of range.");
        }
    }
}

[CustomEditor(typeof(PrefabSelector))]
public class PrefabSelectorEditor : Editor
{
    private int currentPage = 0;

    public override void OnInspectorGUI()
    {
        PrefabSelector prefabSelector = (PrefabSelector)target;

        // Record object for undo purposes
        Undo.RecordObject(prefabSelector, "Prefab Selection Change");

        GUILayout.Label("Select Prefab:");

        if (prefabSelector.prefabVariants.Count > 0)
        {
            int startIndex = currentPage * prefabSelector.elementsPerPage;
            int endIndex = Mathf.Min((currentPage + 1) * prefabSelector.elementsPerPage, prefabSelector.prefabVariants.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                PrefabSelector.PrefabVariant variant = prefabSelector.prefabVariants[i];
                if (variant == null)
                {
                    continue; // Skip null variant objects
                }

                GUILayout.BeginHorizontal();

                foreach (GameObject prefab in variant.variants)
                {
                    if (prefab == null)
                    {
                        continue; // Skip null prefabs
                    }

                    Texture2D texture = AssetPreview.GetAssetPreview(prefab);
                    if (texture == null)
                    {
                        continue; // Skip null textures
                    }

                    Rect rect = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80), GUILayout.Height(80));

                    if (GUI.Button(rect, texture, GUIStyle.none))
                    {
                        int index = prefabSelector.prefabVariants.IndexOf(variant);
                        int variantIndex = variant.variants.IndexOf(prefab);
                        prefabSelector.selectedPrefabIndex = index;
                        prefabSelector.selectedVariantIndex = variantIndex;
                        prefabSelector.SwitchPrefab(index);
                    }

                    if (i == prefabSelector.selectedPrefabIndex && prefabSelector.selectedVariantIndex == variant.variants.IndexOf(prefab))
                    {
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.green;
                        GUI.Label(new Rect(rect.x, rect.y + 60, rect.width, 20), "SELECTED", style);
                    }
                }

                GUILayout.EndHorizontal();
            }

            // Display pagination controls
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Previous", GUILayout.Width(100)))
            {
                currentPage = Mathf.Max(0, currentPage - 1);
            }
            GUILayout.Label($"Page {currentPage + 1}");
            if (GUILayout.Button("Next", GUILayout.Width(100)))
            {
                currentPage = Mathf.Min((prefabSelector.prefabVariants.Count - 1) / prefabSelector.elementsPerPage, currentPage + 1);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("No prefab variants available.", MessageType.Info);
        }

        GUILayout.Space(10);

        // Apply changes to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(prefabSelector);
        }

        DrawDefaultInspector();
    }
}
