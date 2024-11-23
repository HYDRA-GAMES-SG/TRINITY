using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ATrinityCamera : MonoBehaviour
{
    public Camera Camera;
    public ATrinityController Controller; //reference

    public ATrinityFSM CharacterState; //reference
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
