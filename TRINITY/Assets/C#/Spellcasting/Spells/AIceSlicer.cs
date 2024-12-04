using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIceSlicer : MonoBehaviour
{
    Rigidbody Rigidbody;
    public float XRotMultiplier;
    public float YRotMultiplier;
    public float ZRotMultiplier;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(XRotMultiplier, YRotMultiplier, ZRotMultiplier);
    }
}
