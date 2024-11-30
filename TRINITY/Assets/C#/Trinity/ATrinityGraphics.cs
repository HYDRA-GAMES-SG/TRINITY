using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ATrinityGraphics : MonoBehaviour
{
    [HideInInspector] public Animator AnimatorComponent;

    [SerializeField] private ATrinityBrain Brain; // Reference

    [SerializeField] private GameObject ColdParent; // Reference to the parent GameObject containing the mesh

    [SerializeField] private GameObject FireParent; // Reference to the parent GameObject containing the mesh

    [SerializeField] private GameObject LightningParent; // Reference to the parent GameObject containing the mesh

    [SerializeField] private GameObject BlackParent;

    private List<Material> FireMaterials = new List<Material>(); // List to store materials
    private List<Material> ColdMaterials = new List<Material>(); // List to store materials
    private List<Material> LightningMaterials = new List<Material>(); // List to store materials
    private List<Material> BlackMaterials = new List<Material>(); // List to store materials


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

        Brain.OnElementChanged += UpdateMeshColor;
    }

    private void OnDestroy()
    {
        Brain.OnElementChanged -= UpdateMeshColor;
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
    float duration = 1.0f; // Duration of the transition
    float elapsedTime = 0.0f;

    // List to store black materials (if any)
    List<Material> blackMaterials = new List<Material>();
    
    // You might want to populate this list similarly to other material lists in Start()
    // For example: blackMaterials.AddRange(BlackParent.GetComponentsInChildren<Renderer>().SelectMany(r => r.materials));

    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);

        // Use a smooth step interpolation for more natural transition
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // Directly calculate complementary alphas for main materials
        float fadeOutAlpha = 1f - smoothT;
        float fadeInAlpha = smoothT;

        // Special handling for black materials
        float blackAlpha = 0f;
        if (t <= 0.5f)
        {
            // Fade in to full opacity by midpoint
            blackAlpha = Mathf.SmoothStep(0f, 1f, t * 2f);
        }
        else
        {
            // Fade out from midpoint onwards
            blackAlpha = Mathf.SmoothStep(1f, 0f, (t - 0.5f) * 2f);
        }

        // Apply fade-out materials' alpha
        foreach (var material in fadeOutMaterials)
        {
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = fadeOutAlpha;
                material.color = color;
            }
        }

        // Apply fade-in materials' alpha
        foreach (var material in fadeInMaterials)
        {
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = fadeInAlpha;
                material.color = color;
            }
        }

        // Apply black materials' alpha
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

// Helper method to set material alphas
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