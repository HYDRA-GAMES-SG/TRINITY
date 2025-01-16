using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public UHealthComponent Health;
    public UEnemyStatusComponent EnemyStatus;
    public IEnemyController EnemyController;

    public void Start()
    {
        EnemyController = transform.root.Find("Controller").GetComponent<IEnemyController>();
        EnemyStatus = EnemyController.EnemyStatus;
        Health = EnemyStatus.Health;
    }
}
