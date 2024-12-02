using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HierarchyClonerTool : EditorWindow
{
    private GameObject sourceParent;
    private GameObject targetParent;
    private List<string> nameFilters = new List<string>();
    private bool filterByMeshRenderer = false;

    [MenuItem("Tools/Daelonik Artworks/Hierarchy Cloner Tool")]
    public static void ShowWindow()
    {
        GetWindow<HierarchyClonerTool>("Hierarchy Cloner Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hierarchy Cloner Tool", EditorStyles.boldLabel);

        sourceParent = (GameObject)EditorGUILayout.ObjectField("Source Parent", sourceParent, typeof(GameObject), true);
        targetParent = (GameObject)EditorGUILayout.ObjectField("Target Parent", targetParent, typeof(GameObject), true);

        EditorGUILayout.Space();

        GUILayout.Label("Name Filters (one per line)", EditorStyles.boldLabel);
        string filters = EditorGUILayout.TextArea(string.Join("\n", nameFilters), GUILayout.Height(100));
        nameFilters = new List<string>(filters.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries));

        EditorGUILayout.Space();

        filterByMeshRenderer = EditorGUILayout.Toggle("Filter by MeshRenderer", filterByMeshRenderer);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(sourceParent == null || targetParent == null || nameFilters.Count == 0))
        {
            if (GUILayout.Button("Clone Objects"))
            {
                CloneObjects();
            }
        }
    }

    private void CloneObjects()
    {
        if (sourceParent == null || targetParent == null)
        {
            EditorUtility.DisplayDialog("Error", "Source Parent and Target Parent must be assigned.", "OK");
            return;
        }

        // Start recording for Undo
        Undo.SetCurrentGroupName("Clone Objects");
        int undoGroup = Undo.GetCurrentGroup();

        // Find and clone objects
        Transform[] allTransforms = sourceParent.GetComponentsInChildren<Transform>(true);
        HashSet<GameObject> processedObjects = new HashSet<GameObject>();

        foreach (Transform t in allTransforms)
        {
            GameObject go = t.gameObject;
            if (MatchesNameFilters(go.name) && (!filterByMeshRenderer || go.GetComponent<MeshRenderer>() != null))
            {
                if (!processedObjects.Contains(go))
                {
                    // Temporarily detach from parent to get absolute transform
                    Transform originalParent = go.transform.parent;
                    go.transform.SetParent(null);

                    // Store absolute transform
                    Vector3 absolutePosition = go.transform.position;
                    Quaternion absoluteRotation = go.transform.rotation;
                    Vector3 absoluteScale = go.transform.lossyScale;

                    // Reattach to original parent
                    go.transform.SetParent(originalParent);

                    // Clone and apply absolute transform
                    GameObject clone = Instantiate(go);
                    clone.name = go.name; // Ensure the name stays the same.
                    clone.transform.SetParent(targetParent.transform);
                    clone.transform.position = absolutePosition;
                    clone.transform.rotation = absoluteRotation;
                    clone.transform.localScale = absoluteScale;

                    Undo.RegisterCreatedObjectUndo(clone, "Clone Objects");
                    processedObjects.Add(go);
                }
            }
        }

        // Finish recording for Undo
        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.DisplayDialog("Success", "Objects cloned successfully.", "OK");
    }

    private bool MatchesNameFilters(string name)
    {
        foreach (string filter in nameFilters)
        {
            if (name.Contains(filter))
            {
                return true;
            }
        }
        return false;
    }
}
