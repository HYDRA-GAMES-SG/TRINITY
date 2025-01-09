using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    
    [HideInInspector]
    public IAA_TrinityControls Controls;
    public bool bIsStunned => GetAction() == ETrinityAction.ETA_Stunned;
    public bool bCanRotatePlayer => GetAction() != ETrinityAction.ETA_Stunned || GetAction() != ETrinityAction.ETA_Channeling;
    [HideInInspector]
    public bool bForcefieldActive = false;
    [HideInInspector]
    
    private ASpell CurrentSpell;
    private ETrinityElement CurrentElement;
    private ETrinityAction CurrentAction;
    private float StunnedCooldown = 0f;
    
    public event Action<ETrinityElement> OnElementChanged;
    public event Action<ETrinityAction> OnActionChanged;

    void Awake()
    {
        ATrinityGameManager.SetBrain(this);
    }
    
    void Start()
    {
        CurrentElement = ETrinityElement.ETE_Fire;
        CurrentAction = ETrinityAction.ETA_None;

        BindToInputEvents(true);
    }


    void Destroy()
    {
        BindToInputEvents(false);
    }


    void Update()
    {
        HandleStun();
    }

    private void LateUpdate()
    {
        
    }

    private void OnDebugInput()
    {
        
    }
    
    private void HandleStun()
    {
        
        StunnedCooldown -= Time.deltaTime;
        
        if (StunnedCooldown <= 0f && GetAction() == ETrinityAction.ETA_Stunned) //Remove stun after stun duration
        {
            ChangeAction(ETrinityAction.ETA_None);
        }

    }

    public void SetStunnedState(float duration, bool bForcedStun = false) 
    {
        if (bForcefieldActive && !bForcedStun)
        {
            return;
        }

        if (bIsStunned && !bForcedStun)
        {
            return;
        }
        
        if (GetCurrentSpell() != null)
        {
            GetCurrentSpell().Release();
        }
        
        StunnedCooldown = duration;
        ChangeAction(ETrinityAction.ETA_Stunned);

    }

    
    public bool CanAct()
    {
        if (CurrentAction == ETrinityAction.ETA_Stunned)
        {
            return false;
        }

        return true;
    }
    
    public void ModifyState(ETrinityAction newState) 
    {
        CurrentAction = newState;
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
                ATrinityGameManager.GetSpells().PrimaryCold.Cast();
                break;
            case ETrinityElement.ETE_Fire:
                ATrinityGameManager.GetSpells().PrimaryFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                ATrinityGameManager.GetSpells().PrimaryLightning.Cast();
                break;
        }
    }
    public void PrimaryRelease()
    {
        if (!CanAct())
        {
            return;
        }
        switch (GetElement())
        {
            case ETrinityElement.ETE_Cold:
                ATrinityGameManager.GetSpells().PrimaryCold.Release();
                break;
            case ETrinityElement.ETE_Fire:
                break;
            case ETrinityElement.ETE_Lightning:
                ATrinityGameManager.GetSpells().PrimaryLightning.Release();
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
                ATrinityGameManager.GetSpells().SecondaryFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                ATrinityGameManager.GetSpells().SecondaryLightning.Cast();
                break;
        }
    }
    
    public void SecondaryRelease()
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
                ATrinityGameManager.GetSpells().SecondaryFire.Release();
                break;
            case ETrinityElement.ETE_Lightning:
                ATrinityGameManager.GetSpells().SecondaryLightning.Release();
                break;
        }
    }

    public void Utility()
    {
        if (!CanAct())
        {
            return;
        }

        switch (GetElement())
        {
            case ETrinityElement.ETE_Cold:
                ATrinityGameManager.GetSpells().UtilityCold.Cast();
                break;
            case ETrinityElement.ETE_Fire:
                ATrinityGameManager.GetSpells().UtilityFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                ATrinityGameManager.GetSpells().UtilityLightning.Cast();
                break;
        }
    }

    public void Blink()
    {
        if (!CanAct())
        {
            return;
        }
        
        ATrinityGameManager.GetSpells().Blink.Cast();
    }
  
    public void Forcefield()
    {
        if (!CanAct())
        {
            return;
        }
        
        ATrinityGameManager.GetSpells().Forcefield.Cast();
    }
    
    public void ChangeElement(ETrinityElement newElement)
    {
        if (!CanAct())
        {
            return;
        }
        PrimaryRelease();
        SecondaryRelease();
        
        CurrentElement = newElement;
        OnElementChanged?.Invoke(CurrentElement);
    }
    
    public void NextElement()
    {
        if (!CanAct())
        {
            return;
        }

        int intElement = (int)CurrentElement;
        intElement++;
        ETrinityElement newElement = (ETrinityElement)(intElement % Enum.GetValues(typeof(ETrinityElement)).Length);
        ChangeElement(newElement);
    }

    public void PreviousElement()
    {
        if (!CanAct())
        {
            return;
        }

        int intElement = (int)CurrentElement;
        intElement--;
        

        if (intElement < 0)
        {
            intElement = Enum.GetValues(typeof(ETrinityElement)).Length - 1;
        }
        ETrinityElement newElement = (ETrinityElement)intElement;
        ChangeElement(newElement);
    }
    
    public void ChangeAction(ETrinityAction newAction)
    {
        if (newAction != CurrentAction)
        {
            CurrentAction = newAction;
            OnActionChanged?.Invoke(CurrentAction);
        }
    }

    public ETrinityAction GetAction()
    {
        return CurrentAction;
    }

    public ETrinityElement GetElement()
    {
        return CurrentElement;
    }

    public ASpell GetCurrentSpell()
    {
        return CurrentSpell;
    }

    public void SetCurrentSpell(ASpell newSpell)
    {
        if (!CanAct())
        {
            return;
        }
        CurrentSpell = newSpell;

        if (newSpell != null)
        {
            ChangeAction(newSpell.SpellAction);
        }
        else
        {
            ChangeAction(ETrinityAction.ETA_None);
        }
    }
    
    void BindToInputEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityGameManager.GetInput().OnElementalPrimaryPressed += Primary;
            ATrinityGameManager.GetInput().OnElementalSecondaryPressed += Secondary;
            ATrinityGameManager.GetInput().OnElementalUtiltiyPressed += Utility;
            
            ATrinityGameManager.GetInput().OnNextElementPressed += NextElement;
            ATrinityGameManager.GetInput().OnPreviousElementPressed += PreviousElement;
            
            ATrinityGameManager.GetInput().OnBlinkPressed += Blink;
            ATrinityGameManager.GetInput().OnForcefieldPressed += Forcefield;
            ATrinityGameManager.GetInput().OnForcefieldReleased += ATrinityGameManager.GetSpells().Forcefield.Release;
            
            ATrinityGameManager.GetInput().OnElementalPrimaryReleased += PrimaryRelease;
            ATrinityGameManager.GetInput().OnElementalSecondaryReleased += SecondaryRelease;

            ATrinityGameManager.GetInput().OnElementPressed += ChangeElement;
            ATrinityGameManager.GetInput().OnMenuPressed += OnDebugInput;
            return;
        }
        else
        {
            ATrinityGameManager.GetInput().OnElementPressed -= ChangeElement;
            ATrinityGameManager.GetInput().OnElementalPrimaryPressed -= Primary;
            ATrinityGameManager.GetInput().OnElementalSecondaryPressed -= Secondary;
            ATrinityGameManager.GetInput().OnElementalUtiltiyPressed -= Utility;
            ATrinityGameManager.GetInput().OnNextElementPressed -= NextElement;
            ATrinityGameManager.GetInput().OnPreviousElementPressed -= PreviousElement;
            ATrinityGameManager.GetInput().OnBlinkPressed -= Blink;
            ATrinityGameManager.GetInput().OnForcefieldPressed -= Forcefield;

            ATrinityGameManager.GetInput().OnForcefieldReleased -= ATrinityGameManager.GetSpells().Forcefield.Release;
            ATrinityGameManager.GetInput().OnElementalPrimaryReleased -= PrimaryRelease;
            ATrinityGameManager.GetInput().OnElementalSecondaryReleased -= SecondaryRelease;
            ATrinityGameManager.GetInput().OnMenuPressed -= OnDebugInput;
        }
    }
    

    
}
