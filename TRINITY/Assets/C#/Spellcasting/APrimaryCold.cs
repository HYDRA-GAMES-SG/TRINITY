using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryCold : ASpell
{
    Rigidbody Rigidbody;
    private GameObject[] IceSlicer = new GameObject[2];

    public AudioSource ColdSource;
    public AudioClip ColdAttack, ColdSustain, ColdRelease;
    public AUtilityFire UtilityFire;

    // Start is called before the first frame update
    public override void Initialize()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            IceSlicer[i] = Instantiate(SpellPrefab, SpellsReference.CastPoint.position, Quaternion.identity);
            IceSlicer[i].transform.parent = SpellsReference.CastPoint.transform;
            if (i == 1) 
            {
                IceSlicer[i].transform.rotation = new Quaternion(0, -1, 0, 0);
            }
        }
        ColdSource = GetComponent<AudioSource>();
        ColdSource.clip = ColdSustain;
    }
    public void Chill() 
    {

    }
    public override void CastStart()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            if (UtilityFire.bAura)
            {
                AIceSlicer iceSlicer = IceSlicer[i].GetComponent<AIceSlicer>();
                iceSlicer.bAura = true;
            }
            IceSlicer[i].SetActive(true);
        }
        ColdSource.PlayOneShot(ColdAttack);
        ColdSource.Play();
    }

    public override void CastUpdate()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            if (!UtilityFire.bAura & UtilityFire != null)
            {
                AIceSlicer iceSlicer = IceSlicer[i].GetComponent<AIceSlicer>();
                iceSlicer.bAura = false;
            }
        }
    }

    public override void CastEnd()
    {
        for (int i = 0; i < IceSlicer.Length; i++)
        {
            IceSlicer[i].SetActive(false);
        }
        //ColdSource.PlayOneShot(ColdRelease);
        ColdSource.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {


        }
    }
}
