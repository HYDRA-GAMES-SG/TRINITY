using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCubeTrigger : MonoBehaviour
{
    private List<GameObject> ChilledObjects;

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
        if (other.GetComponent<IEnemyController>())
        {
            
        }
        
        if (other.GetComponent<IceWave>())
        {
            if (transform.parent.GetComponent<IceCube>().Mesh.enabled)
            {
                other.gameObject.layer = LayerMask.NameToLayer("IceWave");
            }
        }

    }
}
