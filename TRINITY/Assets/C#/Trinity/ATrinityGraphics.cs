using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class ATrinityGraphics : MonoBehaviour
{
    public GameObject[] StaffObjects;
    public GameObject ClothesParent;
    private SkinnedMeshRenderer[] ClothesMeshes;
    public Material[] ElementMaterials;

    private void Awake()
    {
        ATrinityGameManager.SetGraphics(this);
    }
    
    private void Start()
    {
        ClothesMeshes = ClothesParent.GetComponentsInChildren<SkinnedMeshRenderer>();

        BindtoEvents(true);
        
        UpdateMeshColor(ETrinityElement.ETE_Fire);
        UpdateStaffAura(ETrinityElement.ETE_Fire);
    }

    private void Update()
    {
    }


    public void UpdateMeshColor(ETrinityElement newElement)
    {
        SetClothesMaterials((int)newElement);
    }

    public void UpdateStaffAura(ETrinityElement newElement)
    {
        for (int i = 0; i < StaffObjects.Length; i++)
        {
            if (i == (int)newElement)
            {
                StaffObjects[i].SetActive(true);
            }
            else
            {
                StaffObjects[i].SetActive(false);
            }
            
        }
    }

    public void SetClothesMaterials(int elementMaterialIndex)
    {
        foreach (SkinnedMeshRenderer mesh in ClothesMeshes)
        {
            mesh.material = ElementMaterials[elementMaterialIndex];
        }
    }
    
    private void OnDestroy()
    {
        BindtoEvents(false);
    }

    public void BindtoEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityGameManager.GetBrain().OnElementChanged += UpdateMeshColor;
            ATrinityGameManager.GetBrain().OnElementChanged += UpdateStaffAura;
        }
        else
        {
            ATrinityGameManager.GetBrain().OnElementChanged -= UpdateStaffAura;
            ATrinityGameManager.GetBrain().OnElementChanged -= UpdateMeshColor;
            ATrinityMainMenu.OnMenuElementChanged -= UpdateStaffAura;
            ATrinityMainMenu.OnMenuElementChanged -= UpdateMeshColor;
        }
    }

}