using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightningBeam : MonoBehaviour
{

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;

    private float ChargeStacks = 0f;
    private float IgniteStacks = 0f;

    private bool bIsHitting;
    // Start is called before the first frame update
    void Start()
    {
        IgniteStacks = 0f;
        ChargeStacks = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (!bIsHitting) 
        {
            ImpactVFX.SetActive(false);
        }
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
                bIsHitting = false;
                return;
            }

            bIsHitting = true;
            ImpactVFX.SetActive(true);
            ATrinitySpells spellsRef = ATrinityGameManager.GetSpells();
            
            enemyHitbox.EnemyController.TriggerGetHit();

            ChargeStacks += spellsRef.PrimaryLightning.AilmentStacksPerSecond * Time.deltaTime;

            if (spellsRef.UtilityFire.bAura)
            {
                IgniteStacks += ATrinityGameManager.GetSpells().PrimaryLightning.AilmentStacksPerSecond * Time.deltaTime;
            }

            UEnemyStatusComponent enemyStatus = enemyHitbox.EnemyStatus;
            
            if (ChargeStacks < 1 && IgniteStacks < 1)
            {
                enemyStatus += new FDamageInstance(spellsRef.PrimaryLightning.DamagePerSecond * Time.deltaTime, EAilmentType.EAT_None, 0);
                return;
            }
            else
            {
                if (ChargeStacks > 1)
                {
                    enemyStatus += new FDamageInstance(spellsRef.PrimaryLightning.DamagePerSecond * Time.deltaTime, EAilmentType.EAT_Charge, Mathf.FloorToInt(ChargeStacks));
                    ChargeStacks -= Mathf.FloorToInt(ChargeStacks);
                    //print($"Damage Taken : {Damage}, Ailment type and stacks : {SpellsReference.PrimaryLightning.AilmentType} + {StacksApplied}");
                }

                if (IgniteStacks > 1)
                {
                    if (spellsRef.UtilityFire.bAura && IgniteStacks >= 1f)
                    {
                        enemyStatus += new FDamageInstance(0, EAilmentType.EAT_Ignite, Mathf.FloorToInt(IgniteStacks));
                        IgniteStacks -= Mathf.FloorToInt(IgniteStacks);
                        //print($"Damage Taken : {Damage}, Ailment type and stacks : {EAilmentType.EAT_Ignite} + {StacksApplied}");
                    }
                }
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        bIsHitting = false;
    }
}
