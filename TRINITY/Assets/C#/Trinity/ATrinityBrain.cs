using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ETrinityAction 
{
    ETA_None,
    ETA_Stunned,
    ETA_IFrame,
    ETA_Casting,
    ETA_Channeling
}

public enum ETrinityElement 
{
    ETE_Fire,
    ETE_Cold,
    ETE_Lightning,
    ETE_None
}

public class ATrinityBrain : MonoBehaviour
{
    public float GlobalCooldown = 0f;

    private ATrinitySpells Spells; //reference
    [HideInInspector]
    public ATrinityController Controller; //reference
    public IAA_TrinityControls Controls;
    
    private ETrinityElement Element;
    private ETrinityAction Action;
    private APlayerInput InputReference; //reference


    public event Action<ETrinityElement> OnElementChanged;
    public event Action<ETrinityAction> OnActionChanged;

    void Start()
    {
        Spells = transform.parent.Find("Spells").GetComponent<ATrinitySpells>();
        Controller = transform.parent.Find("Controller").GetComponent<ATrinityController>();

        
        InputReference = GetComponent<APlayerInput>();
        Element = ETrinityElement.ETE_Fire;
        Cursor.lockState = CursorLockMode.Locked;
        
        InputReference.OnElementPressed += ChangeElement;
        InputReference.OnElementalPrimaryPressed += CastPrimarySpell;
        InputReference.OnElementalSecondaryPressed += CastSecondarySpell;
        InputReference.OnElementalUtiltiyPressed += CastUtilitySpell;
        InputReference.OnNextElementPressed += NextElement;
        InputReference.OnPreviousElementPressed += PreviousElement;
        InputReference.OnBlinkPressed += Spells.Blink.CastStart;

        InputReference.OnElementalPrimaryReleased += Spells.PrimaryLightning.CastEnd;

        
    }

    void Destroy()
    {
        InputReference.OnElementPressed -= ChangeElement;
        InputReference.OnElementalPrimaryPressed -= CastPrimarySpell;
        InputReference.OnElementalSecondaryPressed -= CastSecondarySpell;
        InputReference.OnElementalUtiltiyPressed -= CastUtilitySpell;
        InputReference.OnNextElementPressed -= NextElement;
        InputReference.OnPreviousElementPressed -= PreviousElement;
        InputReference.OnBlinkPressed -= Spells.Blink.CastStart;
    
        InputReference.OnElementalPrimaryReleased -= Spells.PrimaryLightning.CastEnd;

    }


    void Update()
    {

    }

    public void CastPrimarySpell()
    {
        if (Action != ETrinityAction.ETA_None)
        {
            return;
        }
        
        if (GlobalCooldown <= 0f)
        {
            switch (GetElement())
            {
                case ETrinityElement.ETE_Cold:
                    //Icicle.CastStart();
                    break;
                case ETrinityElement.ETE_Fire:
                    if (Spells.PrimaryFire.bSpellReady)
                    {
                        Spells.PrimaryFire.CastStart();
                    }
                    break;
                case ETrinityElement.ETE_Lightning:
                    if (Spells.PrimaryLightning.bSpellReady)
                    {
                        Spells.PrimaryLightning.CastStart();
                    }
                    break;
            }
        }
    }

    public void CastSecondarySpell()
    {
        
        if (Action != ETrinityAction.ETA_None)
        {
            return;
        }
    
        if (GlobalCooldown <= 0f)
        {
            switch (GetElement())
            {
                case ETrinityElement.ETE_Cold:
                    break;
                case ETrinityElement.ETE_Fire:
                    break;
                case ETrinityElement.ETE_Lightning:
                    break;
            }
        }
    }

    public void CastUtilitySpell()
    {
        if (Action != ETrinityAction.ETA_None)
        {
            return;
        }

        if (GlobalCooldown <= 0f)
        {
            switch (GetElement())
            {
                case ETrinityElement.ETE_Cold:
                    break;
                case ETrinityElement.ETE_Fire:
                    break;
                case ETrinityElement.ETE_Lightning:
                    break;
            }
        }
    }
    
    public void NextElement()
    {
        int intElement = (int)Element;
        intElement++;
        ETrinityElement newElement = (ETrinityElement)(intElement % Enum.GetValues(typeof(ETrinityElement)).Length);
        ChangeElement(newElement);
        OnElementChanged?.Invoke(newElement);
    }

    public void PreviousElement()
    {
        int intElement = (int)Element;
        intElement--;
        if (intElement < 0)
        {
            intElement = Enum.GetValues(typeof(ETrinityElement)).Length - 1;
        }
        ETrinityElement newElement = (ETrinityElement)intElement;
        ChangeElement(newElement);
        OnElementChanged?.Invoke(newElement);
    }

    public void ChangeElement(ETrinityElement newElement)
    {
        Element = newElement;
        OnElementChanged?.Invoke(Element);
    }

    public void ChangeAction(ETrinityAction newAction)
    {
        if (newAction != Action)
        {
            Action = newAction;
            OnActionChanged?.Invoke(Action);
        }
    }

    public ETrinityAction GetAction()
    {
        return Action;
    }

    public ETrinityElement GetElement()
    {
        return Element;
    }
}
