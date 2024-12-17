using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    static public GameObject Boss;
    
    
    [HideInInspector]
    public ATrinityController Controller; //reference
    public IAA_TrinityControls Controls;
    public bool bIsStunned => GetAction() == ETrinityAction.ETA_Stunned;
    public bool bCanRotatePlayer => GetAction() != ETrinityAction.ETA_Stunned || GetAction() != ETrinityAction.ETA_Channeling;
    [HideInInspector]
    public bool bForcefieldActive = false;
    
    public ATrinitySpells SpellsReference; //reference
    private ASpell CurrentSpell;
    private APlayerInput InputReference; //reference
    private ETrinityElement CurrentElement;
    private ETrinityAction CurrentAction;
    private float StunnedCooldown = 0f;
    
    
    public static event Action<GameObject> OnBossSet;
    public event Action<ETrinityElement> OnElementChanged;
    public event Action<ETrinityAction> OnActionChanged;

    void Start()
    {
        SpellsReference = transform.parent.Find("Spells").GetComponent<ATrinitySpells>();
        Controller = transform.parent.Find("Controller").GetComponent<ATrinityController>();

        
        InputReference = GetComponent<APlayerInput>();
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
        SetStunnedState(3f);
        //SceneManager.LoadScene("CrabBossDungeon");
    }
    
    private void HandleStun()
    {
        StunnedCooldown -= Time.deltaTime;
        
        if (StunnedCooldown < 0 && CurrentAction == ETrinityAction.ETA_Stunned) //Remove stun after stun duration
        {
            ChangeAction(ETrinityAction.ETA_None);
        }

    }

    public void SetStunnedState(float duration) 
    {
        ChangeAction(ETrinityAction.ETA_Stunned);
        StunnedCooldown = duration;
        
        if (GetCurrentSpell() != null)
        {
            GetCurrentSpell().Release();
        }
    }
    
    public bool CanAct()
    {
        if (CurrentAction != ETrinityAction.ETA_None)
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
                SpellsReference.PrimaryCold.Cast();
                break;
            case ETrinityElement.ETE_Fire:
                    SpellsReference.PrimaryFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                    SpellsReference.PrimaryLightning.Cast();
                break;
        }
    }
    public void PrimaryRelease()
    {
        switch (GetElement())
        {
            case ETrinityElement.ETE_Cold:
                SpellsReference.PrimaryCold.Release();
                break;
            case ETrinityElement.ETE_Fire:
                break;
            case ETrinityElement.ETE_Lightning:
                SpellsReference.PrimaryLightning.Release();
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
                SpellsReference.SecondaryFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                break;
        }
    }
    
    public void SecondaryRelease()
    {
        switch (GetElement())
        {
            case ETrinityElement.ETE_Cold:
                break;
            case ETrinityElement.ETE_Fire:
                SpellsReference.SecondaryFire.Release();
                break;
            case ETrinityElement.ETE_Lightning:
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
                break;
            case ETrinityElement.ETE_Fire:
                SpellsReference.UtilityFire.Cast();
                break;
            case ETrinityElement.ETE_Lightning:
                break;
        }
    }

    public void Blink()
    {
        if (!CanAct())
        {
            return;
        }
        
        SpellsReference.Blink.Cast();
    }
    
    public void Forcefield()
    {
        if (!CanAct())
        {
            return;
        }
        
        SpellsReference.Forcefield.Cast();
    }
    
    public void ChangeElement(ETrinityElement newElement)
    {
        CurrentElement = newElement;
        OnElementChanged?.Invoke(CurrentElement);
    }
    
    public void NextElement()
    {
        PrimaryRelease();
        SecondaryRelease();
        
        int intElement = (int)CurrentElement;
        intElement++;
        ETrinityElement newElement = (ETrinityElement)(intElement % Enum.GetValues(typeof(ETrinityElement)).Length);
        OnElementChanged?.Invoke(newElement);
        CurrentElement = newElement;
    }

    public void PreviousElement()
    {
        PrimaryRelease();
        SecondaryRelease();
        
        int intElement = (int)CurrentElement;
        intElement--;
        

        if (intElement < 0)
        {
            intElement = Enum.GetValues(typeof(ETrinityElement)).Length - 1;
        }
        ETrinityElement newElement = (ETrinityElement)intElement;
        OnElementChanged?.Invoke(newElement);
        CurrentElement = newElement;
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
        CurrentSpell = newSpell;

        if (newSpell != null)
        {
            ChangeAction(newSpell.SpellAction);
        }
    }
    
    void BindToInputEvents(bool bBind)
    {
        if (bBind)
        {
            InputReference.OnElementalPrimaryPressed += Primary;
            InputReference.OnElementalSecondaryPressed += Secondary;
            InputReference.OnElementalUtiltiyPressed += Utility;
            
            InputReference.OnNextElementPressed += NextElement;
            InputReference.OnPreviousElementPressed += PreviousElement;
            
            InputReference.OnBlinkPressed += Blink;
            InputReference.OnForcefieldPressed += Forcefield;
            InputReference.OnForcefieldReleased += SpellsReference.Forcefield.Release;
            
            InputReference.OnElementalPrimaryReleased += PrimaryRelease;
            InputReference.OnElementalSecondaryReleased += SecondaryRelease;

            InputReference.OnElementPressed += ChangeElement;
            InputReference.OnMenuPressed += OnDebugInput;
            return;
        }
        else
        {
            InputReference.OnElementPressed -= ChangeElement;
            InputReference.OnElementalPrimaryPressed -= Primary;
            InputReference.OnElementalSecondaryPressed -= Secondary;
            InputReference.OnElementalUtiltiyPressed -= Utility;
            InputReference.OnNextElementPressed -= NextElement;
            InputReference.OnPreviousElementPressed -= PreviousElement;
            InputReference.OnBlinkPressed -= Blink;
            InputReference.OnForcefieldPressed -= Forcefield;

            InputReference.OnForcefieldReleased -= SpellsReference.Forcefield.Release;
            InputReference.OnElementalPrimaryReleased -= PrimaryRelease;
            InputReference.OnElementalSecondaryReleased -= SecondaryRelease;
            InputReference.OnMenuPressed -= OnDebugInput;

        }
    }
    

    public static void SetBoss(GameObject bossObject)
    {
        if (Boss != bossObject && bossObject != null)
        {
            Boss = bossObject;
            OnBossSet?.Invoke(bossObject);
        }
    }
}
