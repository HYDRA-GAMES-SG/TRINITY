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
}

public class ATrinityBrain : MonoBehaviour
{
    public ATrinityCharacter Character; //reference
    public ATrinityCharacter Spells; //reference
    public ATrinityController Controller; //reference
    public IAA_TrinityControls Controls;
    
    private ETrinityElement Element;
    private ETrinityAction Action;
    private APlayerInput InputReference; //reference


    public event Action<ETrinityElement> OnElementChanged;
    public event Action<ETrinityAction> OnActionChanged;

    void Start()
    {
        InputReference = GetComponent<APlayerInput>();
        Element = ETrinityElement.ETE_Fire;
        Cursor.lockState = CursorLockMode.Locked;
        
        InputReference.OnElementPressed += ChangeElement;
        InputReference.OnElementalPrimaryPressed += CastPrimarySpell;
        InputReference.OnElementalSecondaryPressed += CastSecondarySpell;
        InputReference.OnElementalUtiltiyPressed += CastUtilitySpell;
        InputReference.OnNextElementPressed += NextElement;
        InputReference.OnPreviousElementPressed += PreviousElement;
        
        InputReference.OnElementalPrimaryReleased += Spells.LightningBeam.CastEnd;

        
    }

    void Destroy()
    {
        InputReference.OnElementPressed -= ChangeElement;
        InputReference.OnElementalPrimaryPressed -= CastPrimarySpell;
        InputReference.OnElementalSecondaryPressed -= CastSecondarySpell;
        InputReference.OnElementalUtiltiyPressed -= CastUtilitySpell;
        InputReference.OnNextElementPressed -= NextElement;
        InputReference.OnPreviousElementPressed -= PreviousElement;
    
        InputReference.OnElementalPrimaryReleased -= Spells.LightningBeam.CastEnd;

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

        Spells.CastPrimary();
    }

    public void CastSecondarySpell()
    {
        
        if (Action != ETrinityAction.ETA_None)
        {
            return;
        }

        Spells.CastSecondary();
    }

    public void CastUtilitySpell()
    {
        if (Action != ETrinityAction.ETA_None)
        {
            return;
        }

        Spells.CastUtility();
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
