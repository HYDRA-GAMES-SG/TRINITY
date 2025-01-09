using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UManaComponent))]
public class ATrinitySpells : MonoBehaviour
{
    public Vector3 CastDirection => ATrinityGameManager.GetCamera().Camera.transform.forward;

    public Transform CastPoint;

    [Header("Spells")] 
    [HideInInspector] 
    public ASecondaryFire SecondaryFire;
    [HideInInspector] 
    public ASecondaryCold SecondaryCold;
    [HideInInspector]
    public ASecondaryLightning SecondaryLightning;
    [HideInInspector]
    public APrimaryFire PrimaryFire;
    [HideInInspector]
    public APrimaryCold PrimaryCold;
    [HideInInspector]
    public APrimaryLightning PrimaryLightning;
    [HideInInspector]
    public AUtilityFire UtilityFire;
    [HideInInspector] 
    public AUtilityCold UtilityCold;
    [HideInInspector]
    public AUtilityLightning UtilityLightning;
    [HideInInspector]
    public AForcefield Forcefield;
    [HideInInspector] 
    public ABlink Blink;
    
    [HideInInspector]
    public UManaComponent ManaComponent;

    void Awake()
    {
        ATrinityGameManager.SetSpells(this);

    }
    // Start is called before the first frame update
    void Start()
    {
        
        ManaComponent = GetComponent<UManaComponent>();
        SecondaryFire = GetComponentInChildren<ASecondaryFire>();
        SecondaryCold = GetComponentInChildren<ASecondaryCold>();
        SecondaryLightning = GetComponentInChildren<ASecondaryLightning>();
        PrimaryFire = GetComponentInChildren<APrimaryFire>();
        PrimaryCold = GetComponentInChildren<APrimaryCold>();
        PrimaryLightning = GetComponentInChildren<APrimaryLightning>();
        UtilityFire = GetComponentInChildren<AUtilityFire>();
        UtilityCold = GetComponentInChildren<AUtilityCold>();
        UtilityLightning = GetComponentInChildren<AUtilityLightning>();   
        Blink = GetComponentInChildren<ABlink>();
        Forcefield = GetComponentInChildren<AForcefield>();
        
        // need to get component in children for every spell we add
        //have a game object for every spell
    }

    // Update is called once per frame
    void Update()
    {
    }
}
