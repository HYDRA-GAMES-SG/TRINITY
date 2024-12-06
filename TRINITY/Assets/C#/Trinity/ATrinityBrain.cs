using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public float GlobalCooldown = 0f;
    [SerializeField]private float StunnedCooldown = 0f;

    private ASpell CurrentSpell;
    private ATrinitySpells Spells; //reference
    [HideInInspector]
    public ATrinityController Controller; //reference
    public IAA_TrinityControls Controls;
    
    [SerializeField]private ETrinityElement Element;
    [SerializeField]private ETrinityAction Action;
    private APlayerInput InputReference; //reference


    public event Action<ETrinityElement> OnElementChanged;
    public event Action<ETrinityAction> OnActionChanged;

    void Start()
    {
        Spells = transform.parent.Find("Spells").GetComponent<ATrinitySpells>();
        Controller = transform.parent.Find("Controller").GetComponent<ATrinityController>();

        
        InputReference = GetComponent<APlayerInput>();
        Element = ETrinityElement.ETE_Fire;
        
        InputReference.OnElementPressed += ChangeElement;
        InputReference.OnElementalPrimaryPressed += Primary;
        InputReference.OnElementalSecondaryPressed += Secondary;
        InputReference.OnElementalUtiltiyPressed += Utility;
        InputReference.OnNextElementPressed += NextElement;
        InputReference.OnPreviousElementPressed += PreviousElement;
        InputReference.OnBlinkPressed += Blink;
        InputReference.OnForcefieldPressed += Forcefield;

        InputReference.OnForcefieldReleased += Spells.Forcefield.CastEnd;
        InputReference.OnElementalPrimaryReleased += PrimaryRelease;

        
    }

    void Destroy()
    {
        InputReference.OnElementPressed -= ChangeElement;
        InputReference.OnElementalPrimaryPressed -= Primary;
        InputReference.OnElementalSecondaryPressed -= Secondary;
        InputReference.OnElementalUtiltiyPressed -= Utility;
        InputReference.OnNextElementPressed -= NextElement;
        InputReference.OnPreviousElementPressed -= PreviousElement;
        InputReference.OnBlinkPressed -= Blink;
        InputReference.OnForcefieldPressed -= Forcefield;
    
        InputReference.OnForcefieldReleased -= Spells.Forcefield.CastEnd;
        InputReference.OnElementalPrimaryReleased -= PrimaryRelease;

    }


    void Update()
    {
        StunnedCooldown -= Time.deltaTime;
        if (StunnedCooldown < 0 && Action == ETrinityAction.ETA_Stunned) //Remove stun after stun duration
        {
            ChangeAction(ETrinityAction.ETA_None);
            Controller.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SceneManager.LoadScene("CrabBossDungeon");
        }
    }

    public void Stunned(float duration) 
    {
        ChangeAction(ETrinityAction.ETA_Stunned);
        StunnedCooldown = duration;
        Controller.gameObject.SetActive(false);
    }
    public bool CanAct()
    {
        if (Action != ETrinityAction.ETA_None)
        {
            return false;
        }

        // if (GlobalCooldown >= 0f)
        // {
        //     return false;
        // }

        return true;
    }
    public void Primary()
    {
        if (!CanAct())
        {
            return;
        }
        
        switch (GetElement())
        {
            case ETrinityElement.ETE_Cold:
                Spells.PrimaryCold.Cast();
                break;
            case ETrinityElement.ETE_Fire:
                    Spells.PrimaryFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                    Spells.PrimaryLightning.Cast();
                break;
        }
    }
    public void PrimaryRelease()
    {
        switch (GetElement())
        {
            case ETrinityElement.ETE_Cold:
                Spells.PrimaryCold.CastEnd();
                break;
            case ETrinityElement.ETE_Fire:
                break;
            case ETrinityElement.ETE_Lightning:
                Spells.PrimaryLightning.CastEnd();
                break;
        }
    }

    public void Secondary()
    {
        if (!CanAct())
        {
            return;
        }
    
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

    public void Utility()
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

    public void Blink()
    {
        if (GetAction() != ETrinityAction.ETA_Stunned)// && GlobalCooldown >= 0f)
        {
            Spells.Blink.Cast();
        }
    }
    
    public void Forcefield()
    {
        if (GetAction() != ETrinityAction.ETA_Stunned)// && GlobalCooldown >= 0f)
        {
            
            Spells.Forcefield.Cast();
        }
    }
    
    public void NextElement()
    {
        PrimaryRelease();
        int intElement = (int)Element;
        intElement++;
        ETrinityElement newElement = (ETrinityElement)(intElement % Enum.GetValues(typeof(ETrinityElement)).Length);
        ChangeElement(newElement);
        OnElementChanged?.Invoke(newElement);
    }

    public void PreviousElement()
    {
        PrimaryRelease();
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

    public ASpell GetCurrentSpell()
    {
        return CurrentSpell;
    }

    public void SetCurrentSpell(ASpell newSpell)
    {
        CurrentSpell = newSpell;
    }
}
