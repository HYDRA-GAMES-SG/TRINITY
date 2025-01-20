using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTotem : MonoBehaviour
{
    [Header("Totem Properties")] public float Duration;
    public float AttackFrequency;
    public Vector3 InvokePosition;
    public float AttackRange;
    public float SummonDepth;
    public bool bUnsummoned = false;
    public float UnsummonSpeed = 1.5f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        bUnsummoned = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (bUnsummoned)
        {
            float newY = Mathf.Lerp(transform.position.y, InvokePosition.y - SummonDepth, UnsummonSpeed * Time.deltaTime);
            Vector3 newPos = new Vector3(InvokePosition.x, newY, InvokePosition.z);
            transform.localPosition = newPos;
        }
    }

    public void Unsummon()
    {
        //lerp the totem back underground
        bUnsummoned = true;
    }
}
