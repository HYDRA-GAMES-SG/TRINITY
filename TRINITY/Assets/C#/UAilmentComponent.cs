using static UnityEditor.Experimental.GraphView.GraphView;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum EEnemyAilment
{
    EEA_Ignite,
    EEA_Shock,
    EEA_Chill
}

public class UAilmentComponent : MonoBehaviour
{
    public bool bIgnited => AilmentStacks[EEnemyAilment.EEA_Ignite] > 0;
    public bool bShocked => AilmentStacks[EEnemyAilment.EEA_Shock] > 0;
    public bool bChilled => AilmentStacks[EEnemyAilment.EEA_Chill] > 0;

    //public float IgniteDuration = 5f;
    //public float ShockDuration = 5f;
    //public float ChillDuration = 5f;
    public float AilmentDuration = 5f;

    public Dictionary<EEnemyAilment, float> AilmentDurations = new Dictionary<EEnemyAilment, float>();
    private Dictionary<EEnemyAilment, int> AilmentStacks = new Dictionary<EEnemyAilment, int>();
    private Dictionary<EEnemyAilment, float> AilmentTimers = new Dictionary<EEnemyAilment, float>();

    void Start()
    {
        // Loop through each value in the enum
        foreach (EEnemyAilment ailment in Enum.GetValues(typeof(EEnemyAilment)))
        {
            // Add the key & pair value to the dictionary with a default value (e.g., 0)
            AilmentDurations.Add(ailment, AilmentDuration);
            AilmentStacks.Add(ailment, 0);
            AilmentTimers.Add(ailment, 0);
        }
    }

    public void ModifyStack(EEnemyAilment ailment, int stackModifier)
    {
        if (stackModifier > 0)
        {
            AilmentTimers[ailment] = AilmentDurations[ailment];
        }

        AilmentStacks[ailment] += stackModifier;
        int newStackSize = AilmentStacks[ailment];

        if (newStackSize < 0)
        {
            AilmentStacks[ailment] = 0;
        }

        if (newStackSize <= 0)
        {
            AilmentTimers[ailment] = 0f;
        }
    }

    public void RemoveStacks(EEnemyAilment ailment)
    {
        AilmentStacks[ailment] = 0;
    }

    public void Update()
    {
        UpdateAilmentTimers();
    }

    public void UpdateAilmentTimers()
    {
        AilmentTimers[EEnemyAilment.EEA_Ignite] -= Time.deltaTime;
        AilmentTimers[EEnemyAilment.EEA_Chill] -= Time.deltaTime;
        AilmentTimers[EEnemyAilment.EEA_Shock] -= Time.deltaTime;
        UpdateStacks();
    }
    public void UpdateStacks()
    {
        AilmentStacks[EEnemyAilment.EEA_Ignite] = AilmentTimers[EEnemyAilment.EEA_Ignite] <= 0f ? 0 : AilmentStacks[EEnemyAilment.EEA_Ignite];
        AilmentStacks[EEnemyAilment.EEA_Shock] = AilmentTimers[EEnemyAilment.EEA_Shock] <= 0f ? 0 : AilmentStacks[EEnemyAilment.EEA_Shock];
        AilmentStacks[EEnemyAilment.EEA_Chill] = AilmentTimers[EEnemyAilment.EEA_Chill] <= 0f ? 0 : AilmentStacks[EEnemyAilment.EEA_Chill];
    }   
}

