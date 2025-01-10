using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AGUI : MonoBehaviour
{
    public GameObject EnemyHealthBarsParent;
    
    [SerializeField] private Slider HealthSlider, ManaSlider, DamageSlider;

    [SerializeField] private GameObject EnemyHealthBarPrefab;

    
    [Header("UI Objects")]
    public Image FrameBackground;
    public GameObject CurrentElementImage;
    public GameObject[] ElementImages = new GameObject[3];
    public string FireHexademicalCode = "#FF0000";
    public string ColdHexademicalCode = "#00CDFF";
    public string LightningHexademicalCode = "#FFC400";
    private float PlayerHealthTarget;
    private List<IEnemyController> EnemyControllers;

    void Start()
    {
        if (ATrinityGameManager.GetPlayerController() != null)
        {
            ATrinityGameManager.GetPlayerController().HealthComponent.OnHealthModified += UpdateHealthBar;
        }

        if (ATrinityGameManager.GetSpells() != null)
        {
            ATrinityGameManager.GetSpells().ManaComponent.OnManaModified += UpdateManaBar;
        }
        
        CurrentElementImage = ElementImages[(int)ATrinityGameManager.GetBrain().GetElement()];

        if (ATrinityGameManager.GetBrain() != null)
        {
            ATrinityGameManager.GetBrain().OnElementChanged += UpdateElement;
        }
        
        SetupEnemyUI();
    }

    void Update()
    {
        // Lerp DamageSlider and BossDamageSlider to target values
        if (DamageSlider != null)
        {
            DamageSlider.value = Mathf.Lerp(DamageSlider.value, PlayerHealthTarget, Time.deltaTime);
        }
    }

    public void UpdateElement(ETrinityElement newElement)
    {
        CurrentElementImage.SetActive(false); // Turn off previous image

        CurrentElementImage = ElementImages[(int)newElement];
        CurrentElementImage.SetActive(true);

        switch ((int)newElement)
        {
            case 0:
                SetColorByHexademical(FireHexademicalCode);
                break;
            case 1:
                SetColorByHexademical(ColdHexademicalCode);
                break;
            case 2:
                SetColorByHexademical(LightningHexademicalCode);
                break;
        }
    }

    public void SetColorByHexademical(string hexCode)
    {
        if (ColorUtility.TryParseHtmlString(hexCode, out Color newColor))
        {
            FrameBackground.color = newColor;
            Color frameBackgroundColor = FrameBackground.color;
            frameBackgroundColor.a = (40f / 255f);
            FrameBackground.color = frameBackgroundColor;
        }
    }

    private void SetupEnemyUI()
    {
        EnemyControllers = new List<IEnemyController>();
        EnemyControllers = FindObjectsOfType<IEnemyController>().ToList();
        
        for (int i = 0; i < EnemyControllers.Count; i++)
        {
            GameObject go = Instantiate(EnemyHealthBarPrefab, EnemyHealthBarsParent.transform, true);

            AEnemyHealthBar ehb = go.GetComponent<AEnemyHealthBar>();

            ehb.EnemyController = EnemyControllers[i];
            ehb.EnemyName.text = EnemyControllers[i].Name;
            ehb.transform.position = new Vector3(0f, i * -90, 0f);
            ehb.DamageBar.value = 100f;
            ehb.HealthBar.value = 100f;
            ehb.HealthTarget = 100f;
            ehb.gameObject.SetActive(true);
        }
    }

    
    public void UpdateHealthBar(float healthPercent)
    {
        if (HealthSlider != null)
        {
            PlayerHealthTarget = HealthSlider.value; // Record the current value
            HealthSlider.value = healthPercent;
        }
    }

    public void UpdateManaBar(float manaPercent)
    {
        if (ManaSlider != null)
        {
            ManaSlider.value = manaPercent;
        }
    }

    void OnDestroy()
    {
        if (ATrinityGameManager.GetPlayerController() != null)
            ATrinityGameManager.GetPlayerController().HealthComponent.OnHealthModified -= UpdateHealthBar;

        if (ATrinityGameManager.GetSpells() != null)
            ATrinityGameManager.GetSpells().ManaComponent.OnManaModified -= UpdateManaBar;
    }
}