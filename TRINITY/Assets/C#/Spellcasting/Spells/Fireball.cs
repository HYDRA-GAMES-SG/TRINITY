using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ATrinityAudioClip))]
public class Fireball : AProjectile
{
    public ATrinityAudioClip SFX;
    public static APrimaryFire PrimaryFire;
    [HideInInspector]
    public ATrinitySpells Spells;
    [HideInInspector]
    private  Vector3 Direction;
    public float Damage;
    public float Duration;
    public float Speed;
    [HideInInspector]
    public Rigidbody RB;

    public AudioSource FireSource;
    public AudioClip[] FireRelease;

    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;
    public GameObject IgniteVFX;

    // Start is called before the first frame update
    void Start()
    {
        SFX = GetComponent<ATrinityAudioClip>();
        Spells = ATrinityGameManager.GetSpells();
        PrimaryFire = Spells.PrimaryFire;
        RB = GetComponent<Rigidbody>();
        Direction = Spells.CastDirection;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;
        
        if (Duration < 0)
        {
            Despawn();
        }
        
        RB.velocity = Direction * Speed;
    }
    
    public override void Despawn()
    {
        GameObject vfx = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        int i = Random.Range(0, FireRelease.Length - 1);
        FireSource.PlayOneShot(FireRelease[i]);
        Destroy(this.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            UEnemyColliderComponent enemyCollider = collision.gameObject.GetComponent<UEnemyColliderComponent>();

            if (!enemyCollider)
            {
                return;
            }

            enemyCollider.EnemyController.TriggerGetHit();


            UEnemyStatusComponent enemyStatus = enemyCollider.EnemyStatus;
            FDamageInstance damageSource = new FDamageInstance(Damage, PrimaryFire.AilmentType, PrimaryFire.StacksApplied);
            enemyStatus += damageSource;
            //print($"Damage Taken : {Damage}, Ailment type and stacks : {PrimaryFire.AilmentType} + {PrimaryFire.StacksApplied}");
            Despawn();
            
        }
        else 
        {
            Despawn();
        }
    }
}
