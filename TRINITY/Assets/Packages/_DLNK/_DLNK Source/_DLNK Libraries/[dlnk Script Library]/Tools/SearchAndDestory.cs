using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SearchAndDestroy : EditorWindow
{
    private GameObject parentObject;
    private List<string> searchStrings = new List<string>();
    private bool exactMatch = false;
    private string newSearchString = "";

    [MenuItem("Tools/Daelonik Artworks/Search and Destroy")]
    public static void ShowWindow()
    {
        GetWindow<SearchAndDestroy>("Search and Destroy");
    }

    private void OnGUI()
    {
        GUILayout.Label("Search and Destroy Tool", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

        GUILayout.Label("Search Strings");
        newSearchString = EditorGUILayout.TextField("Add Search String", newSearchString);

        if (GUILayout.Button("Add String"))
        {
            if (!string.IsNullOrEmpty(newSearchString) && !searchStrings.Contains(newSearchString))
            {
                searchStrings.Add(newSearchString);
                newSearchString = "";
            }
        }

        GUILayout.Space(10);
        foreach (var searchString in searchStrings)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(searchString);
            if (GUILayout.Button("Remove"))
            {
                searchStrings.Remove(searchString);
                break;
            }
            GUILayout.EndHorizontal();
        }

        exactMatch = EditorGUILayout.Toggle("Exact Match", exactMatch);

        GUILayout.Space(10);
        if (GUILayout.Button("Search and Destroy"))
        {
            if (parentObject != null && searchStrings.Count > 0)
            {
                Undo.RegisterCompleteObjectUndo(parentObject, "Search and Destroy GameObjects");

                // Perform recursive search and destroy
                DestroyObjectsRecursive(parentObject.transform);

                Debug.Log($"Search and Destroy completed for {searchStrings.Count} search strings.");
            }
            else
            {
                Debug.LogWarning("Please specify both a Parent Object and at least one Search String.");
            }
        }
    }

    private void DestroyObjectsRecursive(Transform parentTransform)
    {
        // Use a list to store GameObjects to destroy to avoid modifying the collection during iteration
        List<GameObject> objectsToDestroy = new List<GameObject>();

        // Traverse all children
        foreach (Transform child in parentTransform)
        {
            // Recursively destroy children first
            DestroyObjectsRecursive(child);

            // Check if the current GameObject should be destroyed
            if (ShouldDestroy(child.gameObject))
            {
                objectsToDestroy.Add(child.gameObject);
            }
        }

        // Destroy all collected GameObjects
        foreach (var obj in objectsToDestroy)
        {
            Undo.DestroyObjectImmediate(obj);
        }
    }

    private bool ShouldDestroy(GameObject obj)
    {
        foreach (var searchString in searchStrings)
        {
            bool match = exactMatch ? obj.name == searchString : obj.name.Contains(searchString);
            if (match)
            {
                return true;
            }
        }
        return false;
    }
}
