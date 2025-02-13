
using System.Collections.Generic;
using System;
using UnityEngine;


public class UAilmentComponent : MonoBehaviour
{
    public static float AilmentDuration = 5f;


    public float IgniteDamage => IgniteDamagePerStack * AilmentKeys[EAilmentType.EAT_Ignite].Stacks;
    public float ChillSpeedModifier => 1 - ChillSlowPercentPerStack * AilmentKeys[EAilmentType.EAT_Chill].Stacks;

    public static float GetChargeMoveModifier() => 1 + MaxChargeMoveModifier * ((float)GetChargeStacks() / MAX_STACKS);
    public static float GetChargeAdditionalJumpForce() => MaxJumpModifier * ((float)GetChargeStacks() / MAX_STACKS);
    public static float GetChargeGlideGravityModifier() => 1 - MaxChargeGlideGravityModifier * ((float)GetChargeStacks() / MAX_STACKS);

    public static int MAX_STACKS = 100;

    private static float IgniteDamagePerStack = 1f; //per stack
    private static float ChillSlowPercentPerStack = .004f; //per stack

    private static float MaxChargeMoveModifier = .5f;
    private static float MaxJumpModifier = 22f;
    private static float MaxChargeGlideGravityModifier = .3f;

    public class Ailment
    {
        public int Stacks { get; set; }
        public float Timer { get; set; }
        public float Duration { get; set; }
    }


    public Dictionary<EAilmentType, Ailment> AilmentKeys = new();

    public bool IsAilmentActive(EAilmentType ailment) => AilmentKeys[ailment].Stacks > 0;

    public System.Action<UAilmentComponent> OnAilmentModified;
    public System.Action<UAilmentComponent> OnChargeModified;
    public System.Action<UAilmentComponent> OnIgniteModified;
    public System.Action<UAilmentComponent> OnChillModified;

    private void Update()
    {
        HandleDuration();
        UpdateEffects();
    }

    private void Awake()
    {

        foreach (EAilmentType ailment in Enum.GetValues(typeof(EAilmentType)))
        {
            if (ailment == EAilmentType.EAT_None)
            {
                continue;
            }

            AilmentKeys[ailment] = new Ailment
            {
                Stacks = 0,
                Timer = 0f,
                Duration = AilmentDuration
            };
        }
    }

    private void Start()
    {

    }
    public void UpdateEffects()
    {
        
    }

    public void HandleDuration()
    {
        float deltaTime = Time.deltaTime;

        foreach (EAilmentType ailmentType in AilmentKeys.Keys)
        {
            Ailment ailment = AilmentKeys[ailmentType];

            if (ailment.Stacks > 0)
            {
                ailment.Timer -= deltaTime;

                if (ailment.Timer <= 0f)
                {
                    ailment.Stacks = 0;
                    ailment.Timer = 0f;
                    
                    switch (ailmentType)
                    {
                        case EAilmentType.EAT_Chill:
                            OnChillModified?.Invoke(this);
                            break;
                        case EAilmentType.EAT_Ignite:
                            OnIgniteModified?.Invoke(this);
                            break;
                        case EAilmentType.EAT_Charge:
                            OnChargeModified?.Invoke(this);
                            break;
                    }
                }
            }
        }
    }
    public void ModifyStack(EAilmentType ailmentType, int stackModifier)
    {
        Ailment modifiedAilment = AilmentKeys[ailmentType];

        if (stackModifier > 0)
        {
            modifiedAilment.Timer = modifiedAilment.Duration; // Reset the timer on new stacks.
        }

        int stacksBeforeModifying = modifiedAilment.Stacks;
        
        modifiedAilment.Stacks = Mathf.Clamp(modifiedAilment.Stacks + stackModifier, 0, MAX_STACKS);

        if (modifiedAilment.Stacks != stacksBeforeModifying)
        {
            OnAilmentModified?.Invoke(this);
            
            switch (ailmentType)
            {
                case EAilmentType.EAT_Chill:
                    OnChillModified?.Invoke(this);
                    break;
                case EAilmentType.EAT_Ignite:
                    OnIgniteModified?.Invoke(this);
                    break;
                case EAilmentType.EAT_Charge:
                    OnChargeModified?.Invoke(this);
                    break;
            }
        }
        
        if (modifiedAilment.Stacks == 0)
        {
            modifiedAilment.Timer = 0f;
        }
    }

    public static int GetChargeStacks()
    {
        int chargeStacks = 0;
        
        foreach (IEnemyController ec in ATrinityGameManager.GetEnemyControllers())
        {
            chargeStacks += ec.EnemyStatus.Ailments.AilmentKeys[EAilmentType.EAT_Charge].Stacks;
        }

        return chargeStacks;
    }

    public void RemoveStacks(EAilmentType ailmentType)
    {
        Ailment ailment = AilmentKeys[ailmentType];
        ailment.Stacks = 0;
        ailment.Timer = 0f;
    }

   
}