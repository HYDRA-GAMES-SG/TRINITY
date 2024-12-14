using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class APrimaryCold : ASpell
{
    Rigidbody Rigidbody;
    private GameObject[] IceSlicer = new GameObject[2];

    public AudioSource ColdSource;
    public AudioClip ColdAttack, ColdSustain, ColdRelease;

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
            IceSlicer[i].SetActive(true);
        }
        ColdSource.PlayOneShot(ColdAttack);
        ColdSource.Play();
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
