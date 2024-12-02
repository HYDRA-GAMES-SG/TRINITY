using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChangeMaterials : MonoBehaviour
{
    public Material newMaterial; // The material to be applied
    public int materialIndex = 0; // Index of the material to change

#if UNITY_EDITOR
    [CustomEditor(typeof(ChangeMaterials))]
    public class ChangeMaterialsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ChangeMaterials changeMaterials = (ChangeMaterials)target;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Material at Index"))
            {
                changeMaterials.ChangeMaterialAtIndex();
            }
            if (GUILayout.Button("Change All Materials"))
            {
                changeMaterials.ChangeAllMaterials();
            }
            GUILayout.EndHorizontal();
        }
    }
#endif

    public void ChangeMaterialAtIndex()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            if (materialIndex >= 0 && materialIndex < materials.Length)
            {
                materials[materialIndex] = newMaterial;
                renderer.sharedMaterials = materials;
            }
            else
            {
                Debug.LogWarning("Material index out of range for renderer: " + renderer.gameObject.name);
            }
        }
    }

    public void ChangeAllMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = newMaterial;
            }
            renderer.sharedMaterials = materials;
        }
    }
}
