using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PrimaryLightning : ASpell
{
    public GameObject Beam;
    
    Rigidbody Rigidbody;

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        
        SpellComponent = GetComponent<USpellComponent>();


    }


    public override void Invoke()
    {
        SpellComponent.StartCooldown();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            GameObject impactVFX = Instantiate(ImpactVFX, collision.transform.position, Quaternion.identity);
            if (collision.gameObject.transform.childCount == 0)
            {
                GameObject enemyVFX = Instantiate(ShockVFX, collision.transform.position, Quaternion.identity);
                enemyVFX.transform.parent = collision.transform;
            }
            else
            {
                GameObject enemyObject = collision.transform.GetChild(0).gameObject;
                ParticleSystem enemyVFX = enemyObject.GetComponent<ParticleSystem>();
                enemyVFX.gameObject.SetActive(true);
                enemyVFX.Play();
            }

        }
    }

    public override void CastStart()
    {
        Brain.ChangeAction(ETrinityAction.ETA_Casting);
        
        GameObject spellPrefab;

        if (Beam == null)
        {
            spellPrefab = Instantiate(Beam, TrinitySpells.CastPos.position, Quaternion.Euler(0, 90, 0));
            spellPrefab.transform.parent = Brain.Controller.transform;
            Beam = spellPrefab;
        }
        else 
        {
            Beam.SetActive(true);
        }
    }

    public override void CastEnd()
    {
        Brain.ChangeAction(ETrinityAction.ETA_None);
        
        

        if (Beam != null) 
        {
            Beam.SetActive(false);
        }

    }
}
