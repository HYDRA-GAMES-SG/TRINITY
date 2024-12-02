using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFireball : MonoBehaviour
{
    public ATrinitySpells Spells;
    public Vector3 Direction;
    public float Damage;
    public float Duration;
    public float Speed;
    
    [HideInInspector]
    public Rigidbody RB;
    
    
    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;
    public GameObject IgniteVFX;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        Direction = Spells.CameraRef.transform.forward;
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
    
    public void Ignite()
    {

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
            SpawnExplosion();
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
}
