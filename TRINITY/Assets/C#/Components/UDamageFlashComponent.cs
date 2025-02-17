using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDamageFlashComponent : MonoBehaviour
{
    [HideInInspector]
    UHealthComponent Health;

    [Header("GetHitEffect")]
    [SerializeField] float blinkTimer;
    [SerializeField] float blinkDuration = 0.3f;
    [SerializeField] float blinkIntensity = 0.8f;
    [SerializeField] SkinnedMeshRenderer[] skinnedMeshRenderer;
    Material[] materials;

    [SerializeField] Rigidbody[] RigiBd;
    void Start()
    {
        RigiBd = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in RigiBd)
        {
            if (rb ==RigiBd[0])
            {
                continue;
            }
            rb.isKinematic = true;
            rb.gameObject.AddComponent<UEnemyColliderComponent>();
        }

        Health = GetComponent<UHealthComponent>();

        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        materials = new Material[skinnedMeshRenderer.Length];
        for (int i = 0; i < skinnedMeshRenderer.Length; i++)
        {
            materials[i] = skinnedMeshRenderer[i].material;
            materials[i].EnableKeyword("_EMISSION");
            materials[i].globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        Health.OnDamageTaken += StartBlinking;
    }
    void Update()
    {

    }
    public void StartBlinking(float damageAmount)
    {
        blinkTimer = blinkDuration;
        InvokeRepeating(nameof(HandleBlink), 0f, Time.deltaTime);
    }

    private void StopBlinking()
    {
        CancelInvoke(nameof(HandleBlink));

        foreach (var material in materials)
        {
            if (material != null)
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private void HandleBlink()
    {
        if (blinkTimer <= 0f)
        {
            StopBlinking();
            return;
        }

        blinkTimer -= Time.deltaTime;

        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = lerp * blinkIntensity;

        foreach (var material in materials)
        {
            if (material != null)
            {
                Color emissionColor = Color.white * intensity;
                material.SetColor("_EmissionColor", emissionColor);
            }
        }
    }
}
