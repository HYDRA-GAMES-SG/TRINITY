using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SkyBoxResetter : MonoBehaviour
{
    static SkyBoxResetter()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_Tint", Color.white);
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}
