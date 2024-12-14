using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ATrinityCamera : MonoBehaviour
{
    [HideInInspector]
    public Camera Camera;
    private ATrinityController Controller; // Reference to the player controller
    private APlayerInput InputReference;   // Reference to the input script
    public GameObject LookAtObject;

    void Start()
    {
        Cursor.visible = false;
        Controller = transform.parent.GetComponent<ATrinityController>();
        InputReference = transform.root.Find("Brain").GetComponent<APlayerInput>();
        Camera = GetComponent<Camera>();
    }

    private void OnDestroy()
    {
    }

    void Update()
    {
        //DO NOT USE
    }
    

    void LateUpdate()
    {
    }
}