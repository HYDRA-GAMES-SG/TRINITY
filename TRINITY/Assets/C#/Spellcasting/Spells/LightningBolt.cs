using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{    
    public static ASecondaryLightning SecondaryLightning;
    [HideInInspector]
    private Vector3 Direction;
    private Quaternion BaseRotation;
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
        BaseRotation = Quaternion.Euler(-90, 0, 0);
        RB = GetComponent<Rigidbody>();
        LightningSource = GetComponent<AudioSource>();
        SecondaryLightning = ATrinityGameManager.GetSpells().SecondaryLightning;
        this.transform.SetParent(SecondaryLightning.transform);
        Direction = ATrinityGameManager.GetSpells().CastDirection;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(RB.velocity, Vector3.up) * BaseRotation;
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
