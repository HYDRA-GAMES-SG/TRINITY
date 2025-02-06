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
    [HideInInspector] public Animator AnimatorComponent;

    private void Awake()
    {
        ATrinityGameManager.SetGraphics(this);
    }
    
    private void Start()
    {
        ATrinityGameManager.GetBrain().OnElementChanged += UpdateMeshColor;
        ATrinityGameManager.GetBrain().OnElementChanged += UpdateStaffAura;
        ClothesMeshes = ClothesParent.GetComponentsInChildren<SkinnedMeshRenderer>();

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
        ATrinityGameManager.GetBrain().OnElementChanged -= UpdateStaffAura;
        ATrinityGameManager.GetBrain().OnElementChanged -= UpdateMeshColor;
        ATrinityMainMenu.OnMenuElementChanged -= UpdateStaffAura;
        ATrinityMainMenu.OnMenuElementChanged -= UpdateMeshColor;
    }

}