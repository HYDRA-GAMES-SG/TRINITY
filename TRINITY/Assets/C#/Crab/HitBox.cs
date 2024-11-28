using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public UHealthComponent Health;
   
    public void ModifyHealth(float singleValue)
    {
        Health.Modify(singleValue);
    }
}
