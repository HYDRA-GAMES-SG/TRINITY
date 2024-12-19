using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIceWave : MonoBehaviour
{
    public static APrimaryCold PrimaryCold;
    [HideInInspector]
    public ATrinitySpells Spells;
    [HideInInspector]
    private Vector3 Direction;
    public float Damage;
    public float Duration;
    public float Speed;
    Rigidbody Rigidbody; 

    private EAilmentType AuraAilment;
    public bool bAura;

    public AudioSource ColdSource;
    public AudioClip ColdAttack, ColdSustain, ColdRelease;
    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        PrimaryCold = Spells.PrimaryCold;
        Direction = Spells.CastDirection;
        AuraAilment = EAilmentType.EAT_Ignite;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;

        if (Duration < 0)
        {
            SpawnExplosion();
        }

        Rigidbody.velocity = Direction * Speed;
    }
    public void SpawnExplosion() 
    {
        GameObject vfx = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {

            HitBox enemyHitbox = collision.gameObject.GetComponent<HitBox>();

            if (!enemyHitbox)
            {
                return;
            }

            enemyHitbox.EnemyController.TriggerGetHit();

            FDamageInstance damageSource = new FDamageInstance(Damage, PrimaryCold.AilmentType, PrimaryCold.StacksApplied);
            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus;
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {PrimaryCold.AilmentType} + {PrimaryCold.StacksApplied}");
            if (bAura) 
            {
                FDamageInstance extraAilment = new FDamageInstance(0, AuraAilment, PrimaryCold.StacksApplied);
                enemyStatus = enemyHitbox.EnemyStatus;
                enemyStatus += damageSource;
                print($"Damage Taken : {Damage}, Ailment type and stacks : {AuraAilment} + {PrimaryCold.StacksApplied}");
            }
        }
        else 
        {
            //SpawnExplosion();
        }
    }
}
