using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFireball : MonoBehaviour
{
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
            SpawnExplosion();
        }
        
        RB.velocity = Direction * Speed;
    }
    
    public void SpawnExplosion()
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
            HitBox enemyHitbox = collision.gameObject.GetComponent<HitBox>();

            if (!enemyHitbox)
            {
                return;
            }
            
            enemyHitbox.EnemyController.TriggerGetHit();
            //print(enemyHitbox.EnemyController.name);
            

            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus; 
            FDamageInstance damageSource = new FDamageInstance(Damage, PrimaryFire.AilmentType, PrimaryFire.StacksApplied);
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {PrimaryFire.AilmentType} + {PrimaryFire.StacksApplied}");
            SpawnExplosion();

            //if (collision.gameObject.transform.childCount == 0)
            //{
            //    GameObject enemyVFX = Instantiate(IgniteVFX, collision.transform.position, Quaternion.identity);
            //    enemyVFX.transform.parent = collision.transform;
            //}
            //else
            //{
            //    GameObject enemyObject = collision.transform.GetChild(0).gameObject;
            //    ParticleSystem enemyVFX = enemyObject.GetComponent<ParticleSystem>();
            //    enemyVFX.gameObject.SetActive(true);
            //    enemyVFX.Play();
            //}

        }
    }
}
