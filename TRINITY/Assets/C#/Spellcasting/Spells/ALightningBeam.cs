using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ALightningBeam : MonoBehaviour
{
    Rigidbody RigidBody;
    public float Damage;
    public int StacksApplied;
    public EAilmentType AilmentType;
    private EAilmentType AuraAilment;
    public bool bAura;

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;     

    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();
        AilmentType = EAilmentType.EAT_Charge;
        AuraAilment = EAilmentType.EAT_Ignite;
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

            FDamageInstance damageSource = new FDamageInstance(Damage, AilmentType, StacksApplied);
            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus;
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {AilmentType} + {StacksApplied}");
            if (bAura)
            {
                FDamageInstance extraAilment = new FDamageInstance(0, AuraAilment, StacksApplied);
                enemyStatus = enemyHitbox.EnemyStatus;
                enemyStatus += damageSource;
                print($"Damage Taken : {Damage}, Ailment type and stacks : {AuraAilment} + {StacksApplied}");
            }
        }
    }
}
