using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UManaComponent : MonoBehaviour
{
    [SerializeField]
    private float MAX = 50;
    
    private float Current;
    public System.Action OnManaModified;
    private float Percent => Current / MAX;

    float Modify(float signedValue)
    {

        return Current;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
