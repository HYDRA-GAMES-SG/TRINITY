using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HierarchyOrganizer : EditorWindow
{
    [System.Serializable]
    public class GameObjectStringPair
    {
        public GameObject gameObject;
        public string searchString;
    }

    [System.Serializable]
    public class LODSetting
    {
        public string name;
        public float transitionHeight = 0.5f; // Default transition height
    }

    public GameObject parentGameObject;
    public List<GameObjectStringPair> objectStringPairs = new List<GameObjectStringPair>();
    public List<LODSetting> lodSettings = new List<LODSetting>();
    public bool filterByMeshRenderer = false;
    public bool cloneUnmatched = false;
    public bool autoCreateGameObjects = false;
    public bool removeUnmatchedAfterOrganize = false;
    public bool moveToTransformProxy = false; // New option

    private Dictionary<string, Transform> proxyCache = new Dictionary<string, Transform>();

    [MenuItem("Tools/Daelonik Artworks/Hierarchy Organizer")]
    public static void ShowWindow()
    {
        GetWindow<HierarchyOrganizer>("Hierarchy Organizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Parent GameObject", EditorStyles.boldLabel);
        parentGameObject = (GameObject)EditorGUILayout.ObjectField(parentGameObject, typeof(GameObject), true);

        GUILayout.Label("GameObject-String Pairs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Pair"))
        {
            objectStringPairs.Add(new GameObjectStringPair());
        }

        for (int i = 0; i < objectStringPairs.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (!autoCreateGameObjects)
            {
                objectStringPairs[i].gameObject = (GameObject)EditorGUILayout.ObjectField(objectStringPairs[i].gameObject, typeof(GameObject), true);
            }
            else
            {
                EditorGUILayout.LabelField("Auto-Created");
            }
            objectStringPairs[i].searchString = EditorGUILayout.TextField(objectStringPairs[i].searchString);
            if (GUILayout.Button("Remove"))
            {
                objectStringPairs.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }

        filterByMeshRenderer = EditorGUILayout.Toggle("Filter by MeshRenderer", filterByMeshRenderer);
        cloneUnmatched = EditorGUILayout.Toggle("Clone Unmatched GameObjects", cloneUnmatched);
        autoCreateGameObjects = EditorGUILayout.Toggle("Auto-Create GameObjects", autoCreateGameObjects);
        removeUnmatchedAfterOrganize = EditorGUILayout.Toggle("Remove Unmatched After Organize", removeUnmatchedAfterOrganize);
        moveToTransformProxy = EditorGUILayout.Toggle("Move to Transform Proxy", moveToTransformProxy); // New option

        GUILayout.Label("LOD Settings", EditorStyles.boldLabel);
        if (GUILayout.Button("Add LOD Setting"))
        {
            lodSettings.Add(new LODSetting());
        }

        for (int i = 0; i < lodSettings.Count; i++)
        {
            GUILayout.BeginHorizontal();
            lodSettings[i].name = EditorGUILayout.TextField("Name", lodSettings[i].name);
            lodSettings[i].transitionHeight = EditorGUILayout.Slider("Transition Height", lodSettings[i].transitionHeight, 0f, 1f);
            if (GUILayout.Button("Remove"))
            {
                lodSettings.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Organize Hierarchy"))
        {
            Undo.RegisterCompleteObjectUndo(this, "Organize Hierarchy");
            proxyCache.Clear();
            OrganizeHierarchy();
        }

        if (GUILayout.Button("Auto LOD"))
        {
            Undo.RegisterCompleteObjectUndo(this, "Auto LOD");
            AutoLOD();
        }
    }

    private void OrganizeHierarchy()
    {
        if (parentGameObject == null)
        {
            Debug.LogError("Parent GameObject is not set.");
            return;
        }

        // Create missing GameObjects if the auto-create option is enabled
        if (autoCreateGameObjects)
        {
            foreach (var pair in objectStringPairs)
            {
                if (pair.gameObject == null && !string.IsNullOrEmpty(pair.searchString))
                {
                    GameObject newGameObject = new GameObject(pair.searchString);
                    Undo.RegisterCreatedObjectUndo(newGameObject, "Auto-Create GameObject");
                    newGameObject.transform.SetParent(parentGameObject.transform);
                    pair.gameObject = newGameObject;
                }
            }
        }

        // Gather all current child GameObjects
        Transform[] allChildren = parentGameObject.GetComponentsInChildren<Transform>(true);
        HashSet<Transform> matchedTransforms = new HashSet<Transform>();

        // Process each GameObject-string pair
        foreach (var pair in objectStringPairs)
        {
            if (pair.gameObject == null || string.IsNullOrEmpty(pair.searchString))
            {
                Debug.LogWarning("One of the pairs is missing a GameObject or search string.");
                continue;
            }

            foreach (Transform child in allChildren)
            {
                if (child != parentGameObject.transform && child.name.Contains(pair.searchString))
                {
                    if (!filterByMeshRenderer || (filterByMeshRenderer && child.GetComponent<MeshRenderer>() != null))
                    {
                        Transform targetParent = pair.gameObject.transform;

                        if (moveToTransformProxy)
                        {
                            targetParent = GetOrCreateTransformProxyHierarchy(child, pair.gameObject.transform);
                        }

                        Undo.SetTransformParent(child, targetParent, "Organize Hierarchy");
                        matchedTransforms.Add(child);
                    }
                }
            }
        }

        // Clone unmatched GameObjects if the option is enabled
        if (cloneUnmatched)
        {
            foreach (var pair in objectStringPairs)
            {
                if (pair.gameObject != null)
                {
                    foreach (Transform child in allChildren)
                    {
                        if (!matchedTransforms.Contains(child) &&
                            (!filterByMeshRenderer || (filterByMeshRenderer && child.GetComponent<MeshRenderer>() != null)))
                        {
                            // Clone the GameObject
                            GameObject clone = Instantiate(child.gameObject);
                            Undo.RegisterCreatedObjectUndo(clone, "Organize Hierarchy");

                            // Calculate world transform of the original
                            Vector3 worldPosition = child.position;
                            Quaternion worldRotation = child.rotation;
                            Vector3 worldScale = child.lossyScale;

                            // Set clone's world position, rotation, and scale
                            clone.transform.SetParent(null); // Detach from any parent
                            clone.transform.position = worldPosition;
                            clone.transform.rotation = worldRotation;
                            clone.transform.localScale = worldScale;

                            // Re-parent the clone to the target GameObject
                            Undo.SetTransformParent(clone.transform, pair.gameObject.transform, "Organize Hierarchy");
                        }
                    }
                }
            }
        }

        // Remove unmatched GameObjects if the option is enabled
        if (removeUnmatchedAfterOrganize)
        {
            HashSet<Transform> toRemove = new HashSet<Transform>();

            // Gather all transforms that should be removed
            foreach (Transform child in allChildren)
            {
                if (child != parentGameObject.transform && !matchedTransforms.Contains(child))
                {
                    toRemove.Add(child);
                }
            }

            // Remove each unmatched GameObject
            foreach (Transform child in toRemove)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        Debug.Log("Hierarchy organized successfully.");
    }

    private Transform GetOrCreateTransformProxyHierarchy(Transform originalChild, Transform targetParent)
    {
        List<Transform> proxyHierarchy = new List<Transform>();
        Transform currentTransform = originalChild.parent;

        // Traverse up the hierarchy to find all parents with non-default scales or rotations
        while (currentTransform != parentGameObject.transform)
        {
            if (HasNonDefaultTransform(currentTransform))
            {
                proxyHierarchy.Add(currentTransform);
            }
            currentTransform = currentTransform.parent;
        }

        // Create and set up "Transform Proxy" objects for the hierarchy
        Transform lastCreatedProxy = targetParent;
        proxyHierarchy.Reverse();
        foreach (Transform originalParent in proxyHierarchy)
        {
            string proxyPath = GetFullPath(originalParent) + "_under_" + targetParent.name;
            if (!proxyCache.TryGetValue(proxyPath, out Transform cachedProxy))
            {
                cachedProxy = CreateTransformProxy(originalParent, lastCreatedProxy);
                proxyCache[proxyPath] = cachedProxy;
            }
            lastCreatedProxy = cachedProxy;
        }

        return lastCreatedProxy;
    }

    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null && transform.parent != parentGameObject.transform)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    private Transform CreateTransformProxy(Transform originalParent, Transform targetParent)
    {
        // Create and set up a "Transform Proxy" GameObject
        GameObject transformProxy = new GameObject(originalParent.name + " Transform Proxy");
        Undo.RegisterCreatedObjectUndo(transformProxy, "Create Transform Proxy");
        transformProxy.transform.SetParent(targetParent); // Make it a child of the target game object
        transformProxy.transform.localPosition = originalParent.localPosition;
        transformProxy.transform.localRotation = originalParent.localRotation;
        transformProxy.transform.localScale = originalParent.localScale;
        return transformProxy.transform;
    }

    private bool HasNonDefaultTransform(Transform transform)
    {
        return transform.localScale != Vector3.one || transform.localRotation != Quaternion.identity;
    }

    private void AutoLOD()
    {
        if (parentGameObject == null)
        {
            Debug.LogError("Parent GameObject is not set.");
            return;
        }

        LODGroup lodGroup = parentGameObject.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = Undo.AddComponent<LODGroup>(parentGameObject);
        }

        List<LOD> lods = new List<LOD>();

        foreach (var lodSetting in lodSettings)
        {
            if (!string.IsNullOrEmpty(lodSetting.name))
            {
                Transform lodTransform = parentGameObject.transform.Find(lodSetting.name);
                if (lodTransform != null)
                {
                    List<Renderer> renderers = new List<Renderer>();
                    Transform[] allChildren = lodTransform.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in allChildren)
                    {
                        Renderer renderer = child.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderers.Add(renderer);
                        }
                    }

                    if (renderers.Count > 0)
                    {
                        lods.Add(new LOD(lodSetting.transitionHeight, renderers.ToArray()));
                    }
                }
            }
        }

        lodGroup.SetLODs(lods.ToArray());
        lodGroup.RecalculateBounds();

        Debug.Log("LOD Group created and organized successfully.");
    }
}
