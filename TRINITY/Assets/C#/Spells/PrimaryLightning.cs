using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryLightning : MonoBehaviour
{
    Rigidbody Rigidbody;

    [Header("VFX Prefabs")]
    public GameObject ImpactVFX;
    public GameObject ShockVFX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            GameObject impactVFX = Instantiate(ImpactVFX, collision.transform.position, Quaternion.identity);
            if (collision.gameObject.transform.childCount == 0)
            {
                GameObject enemyVFX = Instantiate(ShockVFX, collision.transform.position, Quaternion.identity);
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
