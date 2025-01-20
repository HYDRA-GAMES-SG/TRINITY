using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    public MeshRenderer Mesh;
    
    private int InitialChillStacks;
    [HideInInspector]
    public float Duration;
    private List<IEnemyController> ChilledEnemies;

    private bool bMelting = false;
    
    // Start is called before the first frame update
    void Start()
    {
        ChilledEnemies = new List<IEnemyController>();
        Duration = ATrinityGameManager.GetSpells().SecondaryCold.Duration;
        InitialChillStacks = ATrinityGameManager.GetSpells().SecondaryCold.InitialChillStacks;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;

        if (Duration <= 0f)
        {
            Reset();
        }

        if (bMelting)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 1.5f * Time.deltaTime);
            if (transform.localScale.magnitude < 0.1f)
            {
                Reset();
            }
        }

    }

    public void Reset()
    {
        gameObject.transform.localScale = Vector3.one;
        gameObject.SetActive(false);
        bMelting = false;
    }
    
    
    public void OnEnemyEnter(IEnemyController other)
    {
        foreach (IEnemyController ec in ChilledEnemies)
        {
            if (other == ec)
            {
                return;
            }
        }

        ChilledEnemies.Add(other);
        other.GetComponent<IEnemyController>().EnemyStatus.Ailments.ModifyStack(EAilmentType.EAT_Chill, InitialChillStacks);
    }

    public void OnEnemyExit(IEnemyController other)
    {
        bool bIsChilled = false;
        foreach (IEnemyController ec in ChilledEnemies)
        {
            if (other == ec)
            {
                bIsChilled = true;
            }
        }

        if (!bIsChilled)
        {
            return;
        }
        
        ChilledEnemies.Remove(other);
        other.GetComponent<IEnemyController>().EnemyStatus.Ailments.ModifyStack(EAilmentType.EAT_Chill, -InitialChillStacks);
    }

    public void Melt()
    {
        bMelting = true;
    }
}
