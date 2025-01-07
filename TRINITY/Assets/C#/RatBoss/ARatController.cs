using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ARatController : IEnemyController
{
    
    [HideInInspector]
    public ARatFSM RatFSM;
    
    [Header("AI Properties")]
    [SerializeField] 
    private float NavSpeed = 7f;
    
    // Start is called before the first frame update
    void Start()
    {
        RatFSM = transform.root.Find("State").GetComponent<ARatFSM>();
        AI = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}