using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightningBeam : MonoBehaviour
{
    [HideInInspector]
    public ATrinitySpells SpellsReference;

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;

    private float ChargeStacks = 0f;
    private float IgniteStacks = 0f;
    // Start is called before the first frame update
    void Start()
    {
        IgniteStacks = 0f;
        ChargeStacks = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //private void OnParticleCollision(GameObject other)
    //{
    //    if (other.gameObject.tag == "Enemy")
    //    {
    //        ACrabHitBox enemyHitbox = other.gameObject.GetComponent<ACrabHitBox>();
    //        enemyHitbox.ApplyDamage(Damage);
    //        UAilmentComponent enemyAilment = other.gameObject.GetComponent<UAilmentComponent>();
    //        enemyAilment.ModifyStack(EAilmentType.EAT_Shock, StacksApplied);
    //    }

    //}
    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.tag == "Enemy")
        //{
        //    GameObject impactVFX = Instantiate(ImpactVFX, collision.transform.position, Quaternion.identity);
        //    if (collision.gameObject.transform.childCount == 0)
        //    {
        //        GameObject enemyVFX = Instantiate(ShockVFX, collision.transform.position, Quaternion.identity);
        //        enemyVFX.transform.parent = collision.transform;
        //    }
        //    else
        //    {
        //        GameObject enemyObject = collision.transform.GetChild(0).gameObject;
        //        ParticleSystem enemyVFX = enemyObject.GetComponent<ParticleSystem>();
        //        enemyVFX.gameObject.SetActive(true);
        //        enemyVFX.Play();
        //    }

        //}
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {

            HitBox enemyHitbox = collision.gameObject.GetComponent<HitBox>();

            if (!enemyHitbox)
            {
                return;
            }

            enemyHitbox.EnemyController.TriggerGetHit();

            ChargeStacks += SpellsReference.PrimaryLightning.AilmentStacksPerSecond * Time.deltaTime;

            if (SpellsReference.UtilityFire.bAura)
            {
                IgniteStacks += SpellsReference.PrimaryLightning.AilmentStacksPerSecond * Time.deltaTime;
            }

            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus;
            
            if (ChargeStacks < 1 && IgniteStacks < 1)
            {
                enemyStatus += new FDamageInstance(SpellsReference.PrimaryLightning.DamagePerSecond * Time.deltaTime, EAilmentType.EAT_None, 0);
                return;
            }
            else
            {
                if (ChargeStacks > 1)
                {
                    enemyStatus += new FDamageInstance(SpellsReference.PrimaryLightning.DamagePerSecond * Time.deltaTime, EAilmentType.EAT_Charge, Mathf.FloorToInt(ChargeStacks));
                    ChargeStacks -= Mathf.FloorToInt(ChargeStacks);
                    //print($"Damage Taken : {Damage}, Ailment type and stacks : {SpellsReference.PrimaryLightning.AilmentType} + {StacksApplied}");
                }

                if (IgniteStacks > 1)
                {
                    if (SpellsReference.UtilityFire.bAura && IgniteStacks >= 1f)
                    {
                        enemyStatus += new FDamageInstance(0, EAilmentType.EAT_Ignite, Mathf.FloorToInt(IgniteStacks));
                        IgniteStacks -= Mathf.FloorToInt(IgniteStacks);
                        //print($"Damage Taken : {Damage}, Ailment type and stacks : {EAilmentType.EAT_Ignite} + {StacksApplied}");
                    }
                }
            }
        }
    }
}
