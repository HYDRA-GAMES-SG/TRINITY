using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IceWave : MonoBehaviour
{
    
    [Header("Ice Wave Properties")]
    public float Damage;
    public float Duration;
    public float Speed;
    
    [HideInInspector]
    public ATrinityController Controller;
    [HideInInspector]
    private APrimaryCold PrimaryCold;
    [HideInInspector]
    public ATrinitySpells Spells;
    [HideInInspector]
    
    public AudioSource ColdSource;
    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;

    private Rigidbody Rigidbody; 
    private Vector3 Direction;
    private Vector3 DesiredVelocity;
    private Quaternion BaseRotation;
    
    

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        PrimaryCold = Spells.PrimaryCold;
        Direction = Spells.CastDirection;
        DesiredVelocity = Direction * Speed;
    
        int shouldRotate = PrimaryCold.CastNumber % 2 == 0 ? 1 : -1;
        
        BaseRotation = Quaternion.Euler(0, 180, shouldRotate * 45);
    
        //Vector3 offset = Controller.Right * 2f * shouldRotate;
        //transform.position += offset;
    }
    
    //offset convergence start
    // void Start()
    // {
    //     PrimaryCold = Spells.PrimaryCold;
    //     int shouldRotate = PrimaryCold.CastNumber % 2 == 0 ? 1 : -1;
    //     Rigidbody = GetComponent<Rigidbody>();
    //     Vector3 offset = Controller.Right * 2f * shouldRotate;
    //     Vector3 originPoint = transform.position + offset;
    //     Vector3 focalPoint = Controller.transform.position + Spells.CastDirection * PrimaryCold.ConvergenceRange;
    //
    //     Direction = focalPoint - originPoint;
    //     
    //     //Direction = Quaternion.LookRotation(Spells.CastDirection, Vector3.up) * Direction;
    //     DesiredVelocity = Direction.normalized * Speed;
    //
    //     
    //     BaseRotation = Quaternion.Euler(0, 180, shouldRotate * 45);
    //
    //
    //     transform.position = originPoint;
    //     
    // }

    void Update()
    {
        Duration -= Time.deltaTime;
        
        transform.rotation = Quaternion.LookRotation(Rigidbody.velocity, Vector3.up) * BaseRotation;
        Rigidbody.velocity = DesiredVelocity;
        
        if (Duration < 0)
        {
            SpawnExplosion();
        }
    }
    
    public void SpawnExplosion() 
    {
        GameObject vfx = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        Destroy(vfx, 5f);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            return;
        }

        if (other.GetComponent<IceWave>())
        {
            SpawnExplosion();
            return;
        }
        
        
        if (other.gameObject.CompareTag("Enemy"))
        {

            HitBox enemyHitbox = other.gameObject.GetComponent<HitBox>();

            if (!enemyHitbox)
            {
                return;
            }

            enemyHitbox.EnemyController.TriggerGetHit();
            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus;
            enemyStatus +=  new FDamageInstance(Damage, EAilmentType.EAT_Chill, PrimaryCold.StacksOfChillApplied);

            if (Spells.UtilityFire.bAura)
            {
                enemyStatus += new FDamageInstance(0f, EAilmentType.EAT_Ignite, PrimaryCold.StacksOfChillApplied);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Vector3 wallNormal = collision.GetContact(0).normal;
            DesiredVelocity -= Vector3.Project(DesiredVelocity, wallNormal) * 2f;
            return;
        }
    }
}
