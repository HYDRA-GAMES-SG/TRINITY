using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AInvincibleBossShield : MonoBehaviour
{
    private Material ShieldMaterial;
    public float HitEffectDuration = 0.2f;
    private float CurrentHitTime = 0f;
    public bool bIsHit = false;
   
    void Start()
    {
        ShieldMaterial = GetComponentInChildren<MeshRenderer>().material;
    }

    void Update()
    {
        if (bIsHit)
        {
            CurrentHitTime += Time.deltaTime;
            Vector4 alphaChannel = ShieldMaterial.GetVector("_MainAlphaChannel");
            alphaChannel.y = Mathf.Lerp(1f, -2f, CurrentHitTime / HitEffectDuration);
            ShieldMaterial.SetVector("_MainAlphaChannel", alphaChannel);

            if (CurrentHitTime >= HitEffectDuration)
            {
                bIsHit = false;
                CurrentHitTime = 0f;
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Spell") ||
            other.gameObject.layer == LayerMask.NameToLayer("IceWave"))
        {
            bIsHit = true;
            CurrentHitTime = 0f;
            Vector4 alphaChannel = ShieldMaterial.GetVector("_MainAlphaChannel");
            alphaChannel.y = 1f;
            ShieldMaterial.SetVector("_MainAlphaChannel", alphaChannel);

            if (other.GetComponent<AProjectile>())
            {
                other.GetComponent<AProjectile>().Despawn();
            }
        }
    }
}
