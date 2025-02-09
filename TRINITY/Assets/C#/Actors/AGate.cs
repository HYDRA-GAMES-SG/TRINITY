using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGate : MonoBehaviour
{
    public float OpenDuration = 3f;
    public float OpenTimer = 0f;
    
    public bool bShouldOpen = false;

    public Vector3 OpenPosition;
    public Vector3 ClosedPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        ClosedPosition = transform.position;
        OpenPosition = ClosedPosition + Vector3.up * 8f;
    }

    // Update is called once per frame
    void Update()
    {
        if (bShouldOpen)
        {
            OpenTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(OpenPosition, ClosedPosition, OpenTimer / OpenDuration);
        }
    }

    public void Open()
    {
        bShouldOpen = true;
    }
}
