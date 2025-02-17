using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDropStaffComponent : MonoBehaviour
{
    
    public float ReturnDuration = .5f;
    public GameObject HeldStaff;
    public GameObject DroppedStaff;

    private Transform StartParent;
    private bool bDropped = false;
    private bool bStaffInPosition = true;

    private float ReturnTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        StartParent = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (ATrinityGameManager.GetPlayerController().HealthComponent.bDead ||
            ATrinityGameManager.GetBrain().bIsStunned)
        {
            DropStaff();
            ReturnTimer = ReturnDuration;
        }
        
        if(bDropped)
        {
            print("Here");
            ReturnTimer -= Time.deltaTime;
            
            ReturnAndHoldStaff();
        }
    }

    void DropStaff()
    {
        if (bStaffInPosition)
        {
            DroppedStaff.transform.SetParent(transform.root);
            DroppedStaff.GetComponent<Rigidbody>().useGravity = true;
            DroppedStaff.GetComponent<BoxCollider>().enabled = true;

            bDropped = true;
            HeldStaff.GetComponent<MeshRenderer>().enabled = false;
            DroppedStaff.SetActive(true);
            bStaffInPosition = false;
        }
    }

    void ReturnAndHoldStaff()
    {
        if (!bDropped)
        {
            return;
        }
        DroppedStaff.GetComponent<Rigidbody>().useGravity = false;
        DroppedStaff.GetComponent<BoxCollider>().enabled = false;
        DroppedStaff.transform.SetParent(StartParent);
        
        if (bStaffInPosition)
        {
            DroppedStaff.SetActive(false);
            HeldStaff.GetComponent<MeshRenderer>().enabled = true;
            bDropped = false;
        }
        else
        {
            DroppedStaff.transform.rotation = Quaternion.Lerp(HeldStaff.transform.rotation, DroppedStaff.transform.rotation, ReturnTimer / ReturnDuration);
            DroppedStaff.transform.position = Vector3.Lerp(HeldStaff.transform.position,
                DroppedStaff.transform.position, ReturnTimer / ReturnDuration);
            
            if (ReturnTimer <= 0)
            {
                bStaffInPosition = true;
            }
        }
    }
}
