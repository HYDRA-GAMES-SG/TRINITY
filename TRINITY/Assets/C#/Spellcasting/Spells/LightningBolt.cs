using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{    
    public static ASecondaryLightning SecondaryLightning;
    [HideInInspector]
    public ATrinitySpells Spells;
    [HideInInspector]
    private Vector3 Direction;
    public float Damage;
    public float Duration;
    public float Speed;
    [HideInInspector]
    public Rigidbody RB;

    public AudioSource LightningSource;

    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        LightningSource = GetComponent<AudioSource>();
        SecondaryLightning = Spells.SecondaryLightning;
        this.transform.SetParent(SecondaryLightning.transform);
        Direction = Spells.CastDirection;
    }

    // Update is called once per frame
    void Update()
    {
        RB.velocity = Direction * Speed;
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
            //print(enemyHitbox.EnemyController.name);


            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus;
            FDamageInstance damageSource = new FDamageInstance(Damage, SecondaryLightning.AilmentType, SecondaryLightning.StacksApplied);
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {SecondaryLightning.AilmentType} + {SecondaryLightning.StacksApplied}");           
           
        }
        //else
        //{
        //    SpawnExplosion();
        //}
    }

}
