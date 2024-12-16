using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIceSlicer : MonoBehaviour
{
    Rigidbody Rigidbody; 

    public float Damage;
    public int StacksApplied;
    public EAilmentType AilmentType;

    public float XRotMultiplier;
    public float YRotMultiplier;
    public float ZRotMultiplier;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        AilmentType = EAilmentType.EAT_Chill;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(XRotMultiplier, YRotMultiplier, ZRotMultiplier);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            HitBox enemyHitbox = collision.gameObject.GetComponent<HitBox>();

            ACrabController enemyController = enemyHitbox.CrabController;
            enemyController.TriggerGetHit();

            FDamageInstance damageSource = new FDamageInstance(Damage, AilmentType, StacksApplied);
            UEnemyStatus enemyStatus = enemyHitbox.EnemyStatus;
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {AilmentType} + {StacksApplied}");
        }
    }
}
