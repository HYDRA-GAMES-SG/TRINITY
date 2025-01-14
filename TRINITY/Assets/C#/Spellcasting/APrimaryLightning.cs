using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryLightning : ASpell
{
    public float DamagePerSecond = 10f;
    public float AilmentStacksPerSecond = 1f;
    Rigidbody Rigidbody;

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;
    
    private GameObject Beam;
    public AudioSource LightningSource;
    public AudioClip LightningAttack, LightningSustain, LightningRelease;
    public AUtilityFire UtilityFire;

    // Start is called before the first frame update
    public override void Initialize()
    {
        if (Beam == null)
        {
            Beam = Instantiate(SpellPrefab, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            Beam.transform.parent = ATrinityGameManager.GetSpells().CastPoint.transform;
        }
        LightningSource = GetComponent<AudioSource>();
        LightningSource.clip = LightningSustain;
    }

    public override void CastStart()
    {
        Beam.SetActive(true);
        //LightningSource.PlayOneShot(LightningAttack);
        LightningSource.Play();
    }

    public override void CastUpdate()
    {
        Quaternion newRot = ATrinityGameManager.GetCamera().transform.rotation * Quaternion.Euler(0f, 80f, 0f);
        Beam.transform.rotation = newRot;
    }

    public override void CastEnd()
    {
        Beam.SetActive(false);
        //LightningSource.PlayOneShot(LightningRelease);
        LightningSource.Stop();
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
