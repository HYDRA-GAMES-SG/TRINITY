using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioClip))]

public class IceCrystal : MonoBehaviour
{
    public AudioClip AudioClip;
    public AudioSource AudioSource;

    [HideInInspector]
    public static AUtilityCold UtilityCold;
    public float Damage;
    Rigidbody Rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        AudioClip = GetComponent<AudioClip>();
        UtilityCold = ATrinityGameManager.GetSpells().UtilityCold;
        Rigidbody = GetComponent<Rigidbody>();


    }

    // Update is called once per frame
    void Update()
    {
        
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
            FDamageInstance damageSource = new FDamageInstance(Damage, UtilityCold.AilmentType, UtilityCold.StacksApplied);
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {UtilityCold.AilmentType} + {UtilityCold.StacksApplied}");
        }
    }
}
