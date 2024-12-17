using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UManaComponent : MonoBehaviour
{
    [SerializeField]
    public float MAX = 50;

    public float Regen = 5f;
    public float Current = 0f;

    public float Percent => Current / MAX;

    public System.Action<float> OnManaModified;
    public System.Action OnOutOfMana;

    public void Awake()
    {
    }
    
    void Start()
    {
        Current = MAX;
    }

    void LateUpdate()
    {
        ApplyRegen();
    }

    public void ApplyRegen() 
    {
        Current += Regen * Time.deltaTime;
        Current = Mathf.Clamp(Current, 0f, MAX);
        OnManaModified?.Invoke(Percent);
    }
    public float Modify(float signedValue)
    {
        Current += signedValue;
        Current = Mathf.Clamp(Current, 0, MAX);

        if (Current <= 0)
        {
            OnOutOfMana?.Invoke();
            Current = 0;
        }

        OnManaModified?.Invoke(Percent);
        return Current;
    }
}
