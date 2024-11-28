using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UManaComponent : MonoBehaviour
{
    [SerializeField]
    public float MAX = 50;
    public float Current;

    private float Percent => Current / MAX;

    public System.Action<float> OnManaModified;

    public void Awake()
    {
    }
    
    void Start()
    {
        Current = MAX;
    }
    float Modify(float signedValue)
    {
        Current += signedValue;
        Current = Mathf.Clamp(Current, 0, MAX);

        if (Current <= 0)
            Current = 0;

        OnManaModified?.Invoke(Percent);
        return Current;
    }
}
