using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public UHealthComponent Health;

    public void ApplyDamage(float damageNumber)
    {
        Health.Modify(-damageNumber);
    }
}
