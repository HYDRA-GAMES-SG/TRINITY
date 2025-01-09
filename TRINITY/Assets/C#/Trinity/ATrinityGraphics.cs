using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ATrinityGraphics : MonoBehaviour
{
    [HideInInspector] public Animator AnimatorComponent;


    [SerializeField] private GameObject ColdParent;

    [SerializeField] private GameObject FireParent;

    [SerializeField] private GameObject LightningParent;

    [SerializeField] private GameObject BlackParent;

    private List<Material> FireMaterials = new List<Material>();
    private List<Material> ColdMaterials = new List<Material>();
    private List<Material> LightningMaterials = new List<Material>();
    private List<Material> BlackMaterials = new List<Material>();


    private void Start()
    {
        if (ColdParent != null)
        {
            Renderer[] renderers = ColdParent.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                ColdMaterials.AddRange(renderer.materials);
            }
        }

        if (FireParent != null)
        {
            Renderer[] renderers = FireParent.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                FireMaterials.AddRange(renderer.materials);
            }
        }

        if (LightningParent != null)
        {
            Renderer[] renderers = LightningParent.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                LightningMaterials.AddRange(renderer.materials);
            }
        }
        
        if (BlackParent != null)
        {
            Renderer[] renderers = BlackParent.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                BlackMaterials.AddRange(renderer.materials);
            }
        }

        ATrinityGameManager.GetBrain().OnElementChanged += UpdateMeshColor;
    }

    private void OnDestroy()
    {
        ATrinityGameManager.GetBrain().OnElementChanged -= UpdateMeshColor;
    }

    private void UpdateMeshColor(ETrinityElement newElement)
    {
        switch (newElement)
        {
            case ETrinityElement.ETE_Fire:
                StartCoroutine(CrossfadeMaterials(ColdMaterials, FireMaterials));
                StartCoroutine(CrossfadeMaterials(LightningMaterials, FireMaterials));
                break;
            case ETrinityElement.ETE_Cold:
                StartCoroutine(CrossfadeMaterials(FireMaterials, ColdMaterials));
                StartCoroutine(CrossfadeMaterials(LightningMaterials, ColdMaterials));
                break;
            case ETrinityElement.ETE_Lightning:
                StartCoroutine(CrossfadeMaterials(ColdMaterials, LightningMaterials));
                StartCoroutine(CrossfadeMaterials(FireMaterials, LightningMaterials));
                break;
        }
    }

private IEnumerator CrossfadeMaterials(List<Material> fadeOutMaterials, List<Material> fadeInMaterials)
{
    float duration = 1.0f;
    float elapsedTime = 0.0f;

    List<Material> blackMaterials = new List<Material>();
    

    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);

        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        float fadeOutAlpha = 1f - smoothT;
        float fadeInAlpha = smoothT;

        // special handling for black materials
        float blackAlpha = 0f;
        if (t <= 0.5f)
        {
            // fade in to full opacity by midpoint
            blackAlpha = Mathf.SmoothStep(0f, 1f, t * 2f);
        }
        else
        {
            // fade out from midpoint onwards
            blackAlpha = Mathf.SmoothStep(1f, 0f, (t - 0.5f) * 2f);
        }

        // apply fade-out material alpha
        foreach (var material in fadeOutMaterials)
        {
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = fadeOutAlpha;
                material.color = color;
            }
        }

        // apply fade-in material alpha
        foreach (var material in fadeInMaterials)
        {
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = fadeInAlpha;
                material.color = color;
            }
        }

        // apply black material alpha
        foreach (var material in blackMaterials)
        {
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = blackAlpha;
                material.color = color;
            }
        }

        yield return null;
    }

    // Ensure final states
    SetMaterialAlphas(fadeOutMaterials, 0f);
    SetMaterialAlphas(fadeInMaterials, 1f);
    SetMaterialAlphas(blackMaterials, 0f);
}

// helper method to set material alphas
private void SetMaterialAlphas(List<Material> materials, float alpha)
{
    foreach (var material in materials)
    {
        if (material.HasProperty("_Color"))
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }
}

}