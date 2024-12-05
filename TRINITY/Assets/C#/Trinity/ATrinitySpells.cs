using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UManaComponent))]
public class ATrinitySpells : MonoBehaviour
{
    public Vector3 CastDirection => CameraRef.LookAtObject.transform.forward;
    public ATrinityCamera CameraRef;
    public ATrinityBrain Brain;
    public Transform CastPoint;
    private APlayerInput InputReference;
    
    [Header("Spells")]
    
    [HideInInspector]
    public APrimaryFire PrimaryFire;
    [HideInInspector]
    public APrimaryCold PrimaryCold;
    [HideInInspector]
    public APrimaryLightning PrimaryLightning;

    [HideInInspector] public AForcefield Forcefield;

    [HideInInspector] 
    public ABlink Blink;
    
    [HideInInspector]
    public UManaComponent ManaComponent;
    
    

    // Start is called before the first frame update
    void Start()
    {
        ManaComponent = GetComponent<UManaComponent>();

        PrimaryFire = GetComponentInChildren<APrimaryFire>();
        PrimaryCold = GetComponentInChildren<APrimaryCold>();
        PrimaryLightning = GetComponentInChildren<APrimaryLightning>();
        Blink = GetComponentInChildren<ABlink>();
        Forcefield = GetComponentInChildren<AForcefield>();

        
        // neeed to get component in children for every spell we add
        //have a game object for every spell
    }

    // Update is called once per frame
    void Update()
    {
    }
}
