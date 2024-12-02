using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScaleInLightmapSetter : MonoBehaviour
{
    public float scaleInLightmap = 1.0f;

    void Start()
    {
        SetScaleInLightmap();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        SetScaleInLightmap();
    }
#endif

    void SetScaleInLightmap()
    {
#if UNITY_EDITOR
        // Get all the Mesh Renderers that are children of this GameObject
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Set the Scale in Lightmap for each Mesh Renderer
        foreach (MeshRenderer renderer in meshRenderers)
        {
            SerializedObject so = new SerializedObject(renderer);
            so.FindProperty("m_ScaleInLightmap").floatValue = scaleInLightmap;
            so.ApplyModifiedProperties();
        }
#endif
    }
}
