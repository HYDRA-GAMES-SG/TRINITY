using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UEnemyStatus : MonoBehaviour
{
    public UHealthComponent Health;
    public UAilmentComponent Ailment;
    // Start is called before the first frame update
    void Start()
    {
        Health = GetComponent<UHealthComponent>();
        Ailment = GetComponent<UAilmentComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
