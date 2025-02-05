using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UHealthComponent))]
[RequireComponent(typeof(UAilmentComponent))]
public class UEnemyStatusComponent : MonoBehaviour
{
    [HideInInspector]
    public UHealthComponent Health;
    [HideInInspector]
    public UAilmentComponent Ailments;
    // Start is called before the first frame update

    void Awake()
    {
        Health = GetComponent<UHealthComponent>();
        Ailments = GetComponent<UAilmentComponent>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Health.Modify(-Ailments.IgniteDamage * Time.deltaTime);
    }
}
