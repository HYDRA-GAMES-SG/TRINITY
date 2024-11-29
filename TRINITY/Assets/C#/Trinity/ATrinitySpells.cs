using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinitySpells : MonoBehaviour
{
    [Header("Spells")]
    public PrimaryFire Fireball;
    public PrimaryCold Icicles;
    public PrimaryLightning LightningBeam;

    [Header("Cooldowns")]
    public float PrimaryCooldown;
    public float SecondaryCooldown;
    public float UtilityCooldown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PrimaryCooldown -= Time.deltaTime;
        SecondaryCooldown -= Time.deltaTime;
        UtilityCooldown -= Time.deltaTime;
    }
}
