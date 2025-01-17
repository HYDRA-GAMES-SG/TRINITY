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
    
    public AudioSource ColdSource;
    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;

    private Rigidbody Rigidbody; 
    private Vector3 Direction;
    private Vector3 DesiredVelocity;
    private Quaternion BaseRotation;

    private int BounceLimit = 4;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        PrimaryCold = ATrinityGameManager.GetSpells().PrimaryCold;
        Direction = ATrinityGameManager.GetSpells().CastDirection;
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
        // if (BounceLimit <= 0)
        // {
        //     SpawnExplosion();
        // }
        
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

            Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
            
            enemyHitbox.EnemyController.TriggerGetHit();
            UEnemyStatusComponent enemyStatus = enemyHitbox.EnemyStatus;
            
            if (ATrinityGameManager.GetSpells().UtilityFire.bAura)
            {
                enemyStatus += new FDamageInstance(0f, EAilmentType.EAT_Ignite, PrimaryCold.StacksOfChillApplied);
            }
            
            enemyStatus +=  new FDamageInstance(Damage, EAilmentType.EAT_Chill, PrimaryCold.StacksOfChillApplied);

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default") || collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Vector3 contactPoint = collision.GetContact(0).point;
            
            // we cast a variety of rays and take the average normal for purposes of projectile reflection
            Vector3 averageNormal = Vector3.zero;
            int hitCount = 0;
            float radius = 1f;
        
            for (int i = 0; i < 8; i++) // Cast 8 rays in a circle
            {
                float angle = i * Mathf.PI * 2f / 8;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Ray ray = new Ray(contactPoint + offset + transform.forward, -transform.forward);
            
                if (Physics.Raycast(ray, out RaycastHit hit, 3f))
                {
                    averageNormal += hit.normal;
                    hitCount++;
                }
            }
        
            if (hitCount > 0)
            {
                averageNormal /= hitCount;
                averageNormal.Normalize();
                DesiredVelocity -= Vector3.Project(DesiredVelocity, averageNormal) * 2f;
            }

            BounceLimit--;
            return;
        }
    }
}
