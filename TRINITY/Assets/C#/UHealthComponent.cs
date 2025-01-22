using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UHealthComponent : MonoBehaviour
{
    [SerializeField]
    public float MAX = 50;

    public float Regen = 5f;
    
    [HideInInspector]
    public float Current = 0f;

    public float Percent => Current / MAX;
    [HideInInspector]
    public bool bDead;

    public System.Action<float> OnDamageTaken;
    public System.Action<float> OnHealthModified;
    public System.Action OnDeath;


    public void Awake()
    {
        
    }

    void Start()
    {
        Current = MAX;
    }

    public void LateUpdate()
    {
        CheckForDeath();
        ApplyRegen();
    }
    
    public float Modify(FDamageInstance damageSource)
    {
        if (bDead) return Current;

        Current -= damageSource.Damage;
        Current = Mathf.Clamp(Current, 0, MAX);

        if (Current <= 0)
        {
            bDead = true;
            Current = 0;
        }

        OnDamageTaken?.Invoke(damageSource.Damage);
        OnHealthModified?.Invoke(Percent);
        return Current;
    }
    
    public float Modify(float signedValue)
    {
        if (bDead) return Current;

        Current += signedValue;
        Current = Mathf.Clamp(Current, 0, MAX);

        if (Current <= 0)
        {
            bDead = true;
            Current = 0;
        }
        
        OnHealthModified?.Invoke(Percent);
        return Current;
    }


    public void ApplyRegen()
    {
        if (!bDead)
        {
            Current += Regen * Time.deltaTime;
            Current = Mathf.Clamp(Current, 0f, MAX);
            OnHealthModified?.Invoke(Percent);
        }
    }
    
    public void CheckForDeath()
    {
        if (Current <= 0f && !bDead)
        {
            bDead = true;
            OnDeath?.Invoke();
        }
    }
}
