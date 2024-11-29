using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryFire : MonoBehaviour, IProjectile
{
    Rigidbody Rigidbody;

    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;
    public GameObject IgniteVFX;

    [Header("Fireball Settings")]
    public float CurrentSpeed;
    public float CurrentDuration;
    public float CurrentDamage;
    public float CurrentMaxRange;
    public float CurrentCooldown;

    public float Speed
    {
        get
        {
            return CurrentSpeed;
        }
        set
        {
            value = CurrentSpeed;
        }
    }
    public float Damage
    {
        get
        {
            return CurrentDamage;
        }
        set
        {
            value = CurrentDamage;
        }
    }
    public float Duration
    {
        get
        {
            return CurrentDuration;
        }
        set
        {
            value = CurrentDuration;
        }
    }
    public float MaxRange { get; set; }
    public float Cooldown 
    {
        get
        {
            return CurrentCooldown;
        }
        set
        {
            value = CurrentCooldown;
        }
    }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentDuration -= Time.deltaTime;
        Rigidbody.velocity = Vector3.forward * CurrentSpeed;
        if (CurrentDuration < 0)
        {
            SelfDestruct();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            SelfDestruct();
            if (collision.gameObject.transform.childCount == 0)
            {
                GameObject enemyVFX = Instantiate(IgniteVFX, collision.transform.position, Quaternion.identity);
                enemyVFX.transform.parent = collision.transform;
            }
            else
            {
                GameObject enemyObject = collision.transform.GetChild(0).gameObject;
                ParticleSystem enemyVFX = enemyObject.GetComponent<ParticleSystem>();
                enemyVFX.gameObject.SetActive(true);
                enemyVFX.Play();
            }

        }
    }
    public void SelfDestruct()
    {
        GameObject vfx = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    public void Ignite()
    {

    }
}
