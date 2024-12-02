using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReplacePrefabsTool : EditorWindow
{
    GameObject parentObject;
    List<GameObjectPair> objectPairs = new List<GameObjectPair>();
    bool searchWholeScene = true;
    bool searchByName = false;
    bool useReplacementNameForSearch = false;
    bool partialNameMatch = false;

    Vector2 scrollPosition;

    [MenuItem("Tools/Daelonik Artworks/Replace Prefabs Tool")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ReplacePrefabsTool));
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Prefabs Tool", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Search Options", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        searchWholeScene = GUILayout.Toggle(searchWholeScene, "Search Whole Scene");
        searchByName = GUILayout.Toggle(searchByName, "Search by Name");
        GUILayout.EndHorizontal();

        if (searchByName)
        {
            EditorGUI.indentLevel++;
            useReplacementNameForSearch = EditorGUILayout.Toggle("Use Replacement Name for Search", useReplacementNameForSearch);
            partialNameMatch = EditorGUILayout.Toggle("Partial Name Match", partialNameMatch);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        if (!searchWholeScene)
        {
            parentObject = EditorGUILayout.ObjectField("Parent GameObject", parentObject, typeof(GameObject), true) as GameObject;
            EditorGUILayout.Space();
        }

        EditorGUILayout.LabelField("Objects to Replace", EditorStyles.boldLabel);

        GUILayout.Space(10);
        GUILayout.Label("Drag and Drop Prefabs Here", EditorStyles.boldLabel);
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Prefabs Here", EditorStyles.helpBox);

        HandleDragAndDrop(dropArea);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < objectPairs.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (searchByName)
            {
                EditorGUILayout.LabelField("Object Name to Replace: " + (objectPairs[i].objectToReplace != null ? objectPairs[i].objectToReplace.name : "Not Set"));
                EditorGUILayout.LabelField("Replacement Name: " + (objectPairs[i].replacementObject != null ? objectPairs[i].replacementObject.name : "Not Set"));
            }
            else
            {
                objectPairs[i].objectToReplace = EditorGUILayout.ObjectField("Object " + (i + 1), objectPairs[i].objectToReplace, typeof(GameObject), true) as GameObject;
            }

            EditorGUILayout.Space();

            objectPairs[i].replacementObject = EditorGUILayout.ObjectField("Replacement " + (i + 1), objectPairs[i].replacementObject, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Switch", GUILayout.Width(80)))
            {
                SwitchGameObjects(i);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove Pair", GUILayout.Width(100)))
            {
                objectPairs.RemoveAt(i);
                GUIUtility.ExitGUI();
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Pair", GUILayout.Width(100)))
        {
            objectPairs.Add(new GameObjectPair());
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Replace Selected Objects", GUILayout.Width(200)))
        {
            Undo.RegisterCompleteObjectUndo(this, "Replace Prefabs Tool");
            ReplaceObjects();
        }

        if (GUILayout.Button("Switch All Objects", GUILayout.Width(200)))
        {
            SwitchAllGameObjects();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Add the Clear List button
        if (GUILayout.Button("Clear List", GUILayout.Width(100)))
        {
            ClearList();
        }
    }

    void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject draggedPrefab = draggedObject as GameObject;
                        if (draggedPrefab != null && PrefabUtility.GetPrefabAssetType(draggedPrefab) == PrefabAssetType.Regular)
                        {
                            // Ensure there is a replacement object
                            if (objectPairs.Count > 0)
                            {
                                GameObjectPair lastPair = objectPairs[objectPairs.Count - 1];
                                if (lastPair.objectToReplace == null)
                                {
                                    lastPair.objectToReplace = draggedPrefab;
                                }
                                else
                                {
                                    objectPairs.Add(new GameObjectPair { objectToReplace = draggedPrefab });
                                }
                            }
                            else
                            {
                                objectPairs.Add(new GameObjectPair { objectToReplace = draggedPrefab });
                            }
                        }
                    }
                }

                Event.current.Use();
            }
        }
    }

    void ReplaceObjects()
    {
        LogObjectPairs();

        if (objectPairs.Count == 0)
        {
            Debug.LogError("Please specify GameObject pairs to replace.");
            return;
        }

        if (searchWholeScene)
        {
            ReplaceObjectsInScene(objectPairs);
        }
        else
        {
            if (parentObject == null)
            {
                Debug.LogError("Please specify a parent GameObject when not searching the whole scene.");
                return;
            }

            foreach (var pair in objectPairs)
            {
                if (searchByName)
                {
                    string searchName = useReplacementNameForSearch && pair.replacementObject != null
                                        ? pair.replacementObject.name
                                        : pair.objectToReplace != null ? pair.objectToReplace.name : "";

                    if (string.IsNullOrEmpty(searchName))
                    {
                        Debug.LogWarning($"Object to replace or replacement object is not set. Pair index: {objectPairs.IndexOf(pair)}");
                        continue;
                    }

                    ReplaceObjectsInHierarchyByName(parentObject.transform, searchName, pair.replacementObject);
                }
                else
                {
                    if (pair.objectToReplace != null && pair.replacementObject != null)
                    {
                        ReplaceObjectsInHierarchy(parentObject.transform, pair.objectToReplace, pair.replacementObject);
                    }
                    else
                    {
                        Debug.LogWarning($"Object to replace or replacement object is not set for pair. Object to Replace: {pair.objectToReplace}, Replacement Object: {pair.replacementObject}");
                    }
                }
            }
        }

        Debug.Log("Replacement complete.");
    }

    void ReplaceObjectsInScene(List<GameObjectPair> pairs)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var pair in pairs)
        {
            if (searchByName && useReplacementNameForSearch)
            {
                if (pair.replacementObject == null)
                {
                    Debug.LogWarning($"Replacement object is missing for pair. Skipping replacement. Pair index: {objectPairs.IndexOf(pair)}");
                    continue;
                }

                string searchName = pair.replacementObject.name;

                if (string.IsNullOrEmpty(searchName))
                {
                    Debug.LogWarning($"Search name is empty for replacement object: {pair.replacementObject}. Skipping replacement.");
                    continue;
                }

                foreach (GameObject obj in allObjects)
                {
                    if (obj == null) continue;

                    if (partialNameMatch)
                    {
                        if (obj.name.Contains(searchName))
                        {
                            Debug.Log($"Replacing object: {obj.name} with replacement: {pair.replacementObject.name}");
                            ReplaceGameObject(obj, pair.replacementObject);
                        }
                    }
                    else
                    {
                        if (obj.name == searchName)
                        {
                            Debug.Log($"Replacing object: {obj.name} with replacement: {pair.replacementObject.name}");
                            ReplaceGameObject(obj, pair.replacementObject);
                        }
                    }
                }
            }
            else
            {
                if (pair.objectToReplace == null || pair.replacementObject == null)
                {
                    Debug.LogWarning($"Skipping replacement because objectToReplace or replacementObject is missing for pair. Object to Replace: {pair.objectToReplace}, Replacement Object: {pair.replacementObject}");
                    continue;
                }

                string searchName = pair.objectToReplace.name;

                if (string.IsNullOrEmpty(searchName))
                {
                    Debug.LogWarning($"Search name is empty for object to replace: {pair.objectToReplace}. Skipping replacement.");
                    continue;
                }

                foreach (GameObject obj in allObjects)
                {
                    if (obj == null) continue;

                    if (partialNameMatch)
                    {
                        if (obj.name.Contains(searchName))
                        {
                            Debug.Log($"Replacing object: {obj.name} with replacement: {pair.replacementObject.name}");
                            ReplaceGameObject(obj, pair.replacementObject);
                        }
                    }
                    else
                    {
                        if (obj.name == searchName)
                        {
                            Debug.Log($"Replacing object: {obj.name} with replacement: {pair.replacementObject.name}");
                            ReplaceGameObject(obj, pair.replacementObject);
                        }
                    }
                }
            }
        }
    }

    void ReplaceObjectsInHierarchy(Transform parent, GameObject objectToReplace, GameObject replacement)
    {
        if (objectToReplace == null || replacement == null)
        {
            Debug.LogWarning("Object to replace or replacement object is missing.");
            return;
        }

        List<GameObject> objectsToReplace = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child == null) continue;

            if (PrefabUtility.GetPrefabInstanceStatus(child.gameObject) == PrefabInstanceStatus.Connected)
            {
                GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (prefabRoot == objectToReplace)
                {
                    objectsToReplace.Add(child.gameObject);
                }
            }
            else if (child.gameObject.name == objectToReplace.name && searchByName)
            {
                objectsToReplace.Add(child.gameObject);
            }

            ReplaceObjectsInHierarchy(child, objectToReplace, replacement);
        }

        foreach (GameObject obj in objectsToReplace)
        {
            if (obj == null) continue;
            ReplaceGameObject(obj, replacement);
        }
    }

    void ReplaceObjectsInHierarchyByName(Transform parent, string objectNameToReplace, GameObject replacement)
    {
        if (replacement == null)
        {
            Debug.LogWarning("Replacement object is missing.");
            return;
        }

        List<GameObject> objectsToReplace = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child == null) continue;

            bool nameMatches = partialNameMatch ? child.name.Contains(objectNameToReplace) : child.name == objectNameToReplace;

            if (nameMatches)
            {
                objectsToReplace.Add(child.gameObject);
            }
            else
            {
                ReplaceObjectsInHierarchyByName(child, objectNameToReplace, replacement);
            }
        }

        foreach (GameObject obj in objectsToReplace)
        {
            if (obj == null) continue;
            ReplaceGameObject(obj, replacement);
        }
    }

    void ReplaceGameObject(GameObject obj, GameObject replacement)
    {
        if (obj == null || replacement == null)
        {
            Debug.LogWarning("Object or replacement prefab is missing.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(obj, "Replace Prefab");
        GameObject newObject = PrefabUtility.InstantiatePrefab(replacement) as GameObject;
        Undo.RegisterCreatedObjectUndo(newObject, "Instantiate Replacement");

        newObject.transform.SetParent(obj.transform.parent);
        newObject.transform.localPosition = obj.transform.localPosition;
        newObject.transform.localRotation = obj.transform.localRotation;
        newObject.transform.localScale = obj.transform.localScale;

        Undo.DestroyObjectImmediate(obj);
    }

    void SwitchGameObjects(int index)
    {
        GameObject temp = objectPairs[index].objectToReplace;
        objectPairs[index].objectToReplace = objectPairs[index].replacementObject;
        objectPairs[index].replacementObject = temp;
    }

    void SwitchAllGameObjects()
    {
        for (int i = 0; i < objectPairs.Count; i++)
        {
            SwitchGameObjects(i);
        }
    }

    void ClearList()
    {
        objectPairs.Clear();
    }

    void LogObjectPairs()
    {
        for (int i = 0; i < objectPairs.Count; i++)
        {
            var pair = objectPairs[i];
            Debug.Log($"Pair {i}: Object to Replace = {(pair.objectToReplace != null ? pair.objectToReplace.name : "null")}, Replacement Object = {(pair.replacementObject != null ? pair.replacementObject.name : "null")}");
        }
    }

    [System.Serializable]
    public class GameObjectPair
    {
        public GameObject objectToReplace;
        public GameObject replacementObject;
    }
}
