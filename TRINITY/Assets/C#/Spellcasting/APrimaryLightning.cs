using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryLightning : ASpell
{
    Rigidbody Rigidbody;

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;

    private GameObject Beam;
    // Start is called before the first frame update
    public override void Initialize()
    {
        if (Beam == null)
        {
            Beam = Instantiate(SpellPrefab, Spells.CastPoint.position, Quaternion.identity);
            Beam.transform.parent = Spells.CastPoint.transform;
        }
    }

    public override void CastStart()
    {
        Beam.SetActive(true);
    }

    public override void CastUpdate()
    {
        Quaternion newRot = Spells.CameraRef.transform.rotation * Quaternion.Euler(0f, 80f, 0f);
        Beam.transform.rotation = newRot;

    }

    public override void CastEnd()
    {
        Beam.SetActive(false);
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
}
