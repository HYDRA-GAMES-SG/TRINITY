using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UAilmentComponent;

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
        if (!Health.bInvulnerable)
        {
            Health.Modify(-Ailments.IgniteDamage * Time.deltaTime);
        }
        if (Health.bInvulnerable)
        {
            foreach (EAilmentType ailmentType in Ailments.AilmentKeys.Keys)
            {
                Ailment modifiedAilment = Ailments.AilmentKeys[ailmentType];
                Ailments.ModifyStack(ailmentType, -modifiedAilment.Stacks);
            }
        }
    }
}
