using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCastPoint : MonoBehaviour
{
    public Transform CastPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = CastPoint.transform.position;
        transform.rotation = CastPoint.transform.rotation;
    }
}
