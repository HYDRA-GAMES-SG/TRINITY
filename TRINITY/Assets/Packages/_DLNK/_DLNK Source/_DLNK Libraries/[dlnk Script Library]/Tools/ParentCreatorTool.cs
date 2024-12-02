using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ParentCreatorTool : EditorWindow
{
    private bool copyRotation = false;
    private bool copyScale = false;
    private bool useOriginalName = false;
    private string parentName = "Parent";

    [MenuItem("Tools/Daelonik Artworks/Parent Creator Tool")]
    public static void ShowWindow()
    {
        GetWindow<ParentCreatorTool>("Parent Creator Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Parent Creator Tool", EditorStyles.boldLabel);

        useOriginalName = EditorGUILayout.Toggle("Use Original Name", useOriginalName);
        if (!useOriginalName)
        {
            parentName = EditorGUILayout.TextField("Parent Name", parentName);
        }

        copyRotation = EditorGUILayout.Toggle("Copy Rotation", copyRotation);
        copyScale = EditorGUILayout.Toggle("Copy Scale", copyScale);

        if (GUILayout.Button("Create Parents"))
        {
            CreateParents();
        }
    }

    private void CreateParents()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select at least one object in the hierarchy.", "OK");
            return;
        }

        // List to keep track of the newly created parent objects
        List<GameObject> newParents = new List<GameObject>();

        foreach (GameObject selected in Selection.gameObjects)
        {
            // Determine parent name
            string name = useOriginalName ? selected.name : parentName;

            // Create a new parent GameObject
            GameObject parent = new GameObject(name);

            // Set parent position, rotation, and scale
            parent.transform.position = selected.transform.position;
            if (copyRotation)
            {
                parent.transform.rotation = selected.transform.rotation;
            }
            else
            {
                parent.transform.rotation = Quaternion.identity;
            }
            if (copyScale)
            {
                parent.transform.localScale = selected.transform.localScale;
            }
            else
            {
                parent.transform.localScale = Vector3.one;
            }

            // Set the parent of the selected object
            Undo.SetTransformParent(selected.transform, parent.transform, "Create Parent");

            // Add the parent to the list of new parents
            newParents.Add(parent);
        }

        // Select only the newly created parent objects
        Selection.objects = newParents.ToArray();
    }
}
