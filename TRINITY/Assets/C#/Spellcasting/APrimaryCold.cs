using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryCold : ASpell
{
    Rigidbody Rigidbody;
    private GameObject[] IceSlicer = new GameObject[2];
    // Start is called before the first frame update
    public override void Initialize()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            IceSlicer[i] = Instantiate(SpellPrefab, Spells.CastPos.position, Quaternion.identity);
            IceSlicer[i].transform.parent = Spells.CastPos.transform;
            if (i == 1) 
            {
                IceSlicer[i].transform.rotation = new Quaternion(0, -1, 0, 0);
            }
        }
    }    

    public override void CastStart()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            IceSlicer[i].SetActive(true);
        }
    }

    public override void CastUpdate()
    {

    }

    public override void CastEnd()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            IceSlicer[i].SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {


        }
    }
}
