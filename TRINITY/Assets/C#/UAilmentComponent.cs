using static UnityEditor.Experimental.GraphView.GraphView;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum EAilmentType
{
    EAT_Ignite,
    EAT_Shock,
    EAT_Chill
}

public class UAilmentComponent : MonoBehaviour
{
    public class Ailment
    {
        public int Stacks { get; set; }
        public float Timer { get; set; }
        public float Duration { get; set; }
    }

    public float AilmentDuration = 5f;

    public Dictionary<EAilmentType, Ailment> Ailments = new();

    public bool IsAilmentActive(EAilmentType ailment) => Ailments[ailment].Stacks > 0;

    private void Start()
    {
        foreach (EAilmentType ailment in Enum.GetValues(typeof(EAilmentType)))
        {
            Ailments[ailment] = new Ailment
            {
                Stacks = 0,
                Timer = 0f,
                Duration = AilmentDuration
            };
        }
    }

    public void ModifyStack(EAilmentType ailment, int stackModifier)
    {
        var data = Ailments[ailment];

        if (stackModifier > 0)
        {
            data.Timer = data.Duration; // Reset the timer on new stacks.
        }

        data.Stacks = Mathf.Max(0, data.Stacks + stackModifier);

        if (data.Stacks == 0)
        {
            data.Timer = 0f;
        }
    }

    public void RemoveStacks(EAilmentType ailmentType)
    {
        Ailment ailment = Ailments[ailmentType];
        ailment.Stacks = 0;
        ailment.Timer = 0f;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (EAilmentType ailmentType in Ailments.Keys)
        {
            Ailment ailment = Ailments[ailmentType];

            if (ailment.Stacks > 0)
            {
                ailment.Timer -= deltaTime;

                if (ailment.Timer <= 0f)
                {
                    ailment.Stacks = 0;
                    ailment.Timer = 0f;
                }
            }
        }
    }
}