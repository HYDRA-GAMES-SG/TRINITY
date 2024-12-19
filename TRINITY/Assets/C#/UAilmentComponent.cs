
using System.Collections.Generic;
using System;
using UnityEngine;

public enum EAilmentType
{
    EAT_Ignite,
    EAT_Charge,
    EAT_Chill,
    EAT_None
}

public class UAilmentComponent : MonoBehaviour
{
    public int MAX_STACKS = 100;
    public float IgniteDamage => IgniteDamagePerStack * AilmentKeys[EAilmentType.EAT_Ignite].Stacks;
    public float ChillSpeedModifier => 1 - ChillSlowPercentPerStack * AilmentKeys[EAilmentType.EAT_Chill].Stacks;
    public float ChargeMoveModifier => 1 + ChargeMoveModifierPerStack * AilmentKeys[EAilmentType.EAT_Charge].Stacks;
    public float ChargeAdditionalJumpForce => ChargeJumpForcePerStack * AilmentKeys[EAilmentType.EAT_Charge].Stacks;
    public float ChargeGlideGravityModifier => 1 - ChargeGlideGravityModifierPerStack * AilmentKeys[EAilmentType.EAT_Charge].Stacks;
    
    [SerializeField]
    private float IgniteDamagePerStack = 1f;
    [SerializeField]
    private float ChillSlowPercentPerStack = .004f;
    [SerializeField]
    private float ChargeMoveModifierPerStack = .2f;
    [SerializeField]
    private float ChargeJumpForcePerStack = .02f;
    [SerializeField]
    private float ChargeGlideGravityModifierPerStack = .001f;
    
    public class Ailment
    {
        public int Stacks { get; set; }
        public float Timer { get; set; }
        public float Duration { get; set; }
    }

    public float AilmentDuration = 5f;

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
    
    private void Start()
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

    public void RemoveStacks(EAilmentType ailmentType)
    {
        Ailment ailment = AilmentKeys[ailmentType];
        ailment.Stacks = 0;
        ailment.Timer = 0f;
    }

   
}