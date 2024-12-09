using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFireball : MonoBehaviour
{
    [HideInInspector]
    public ATrinitySpells Spells;
    [HideInInspector]
    private  Vector3 Direction;
    public float Damage;
    public float Duration;
    public float Speed;
    public int StacksApplied;
    public EAilmentType AilmentType;
    [HideInInspector]
    public Rigidbody RB;
    
    
    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;
    public GameObject IgniteVFX;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        Direction = Spells.CastDirection;
        AilmentType = EAilmentType.EAT_Ignite;
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
        Destroy(this.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            ACrabHitBox enemyHitbox = collision.gameObject.GetComponent<ACrabHitBox>();
            //enemyHitbox.ApplyDamage(Damage);
            //UAilmentComponent enemyAilment = collision.gameObject.GetComponent<UAilmentComponent>();
            //enemyAilment.ModifyStack(EAilmentType.EAT_Ignite, StacksApplied);
            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus; 
            FDamageInstance damageSource = new FDamageInstance(Damage, AilmentType, StacksApplied);
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {AilmentType} + {StacksApplied}");
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
