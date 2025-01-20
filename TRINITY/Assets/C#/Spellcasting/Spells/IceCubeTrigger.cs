using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCubeTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HitBox>())
        {
            IEnemyController enemyController = other.GetComponent<HitBox>().EnemyController;

            if (other.transform == enemyController.CoreCollider)
            {
                transform.parent.GetComponent<IceCube>().OnEnemyEnter(enemyController);
            }

            return;
        }
        
        if (other.GetComponent<IceWave>())
        {
            if (transform.parent.GetComponent<IceCube>().Mesh.enabled)
            {
                other.gameObject.layer = LayerMask.NameToLayer("IceWave");
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HitBox>())
        {
            IEnemyController enemyController = other.GetComponent<HitBox>().EnemyController;

            if (other.transform == enemyController.CoreCollider)
            {
                transform.parent.GetComponent<IceCube>().OnEnemyExit(enemyController);
            }

            return;
        }
    }
}
