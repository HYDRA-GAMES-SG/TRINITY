using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UHealthComponent : MonoBehaviour
{
    [SerializeField]
    public float MAX = 50;

    public float Regen = 5f;
    public float Current = 0f;

    public float Percent => Current / MAX;
    public bool bDead;

    [Header("Status Effect Stacks")]
    [SerializeField]
    private float IgniteStacks;
    [SerializeField]
    private float ChillStacks;
    [SerializeField]
    private float ShockStacks;

    [Header ("Status Effects")]
    public bool bIgnited;
    public bool bChilled;
    public bool bShocked;

    public System.Action<float> OnHealthModified;
    public System.Action OnDeath;


    public void Awake()
    {
        
    }

    void Start()
    {
        Current = MAX;
    }

    public void Update()
    {
        CheckForDeath();
        ApplyRegen();
        ApplyStatusEffect();
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
        }
    }
    public void ApplyStatusEffect() 
    {
        if (bIgnited) 
        {
            //Deals damage over time (DOT) 
        }
        if (bChilled)
        {
            //Slows movement speed (MS)
        }
        if (bShocked)
        {
            //TBD
        }
    }
    public void CheckForDeath()
    {
        if (Current <= 0f)
        {
            bDead = true;
            OnDeath?.Invoke();
        }
    }
}
