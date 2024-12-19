using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jerome
{
    public class MonsterGuardCollider : MonoBehaviour
    {
        public float Damage;
        BoxCollider BoxCollider;
        public float ColliderTimer;
        public float OffTime;
        // Start is called before the first frame update
        void Start()
        {
            BoxCollider = GetComponent<BoxCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            ColliderTimer -= Time.deltaTime;
            if (ColliderTimer < 0)
            {
                BoxCollider.enabled = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();
                player.HealthComponent.Modify(Damage);
                print($"Damage dealt to Player : {Damage}");
                BoxCollider.enabled = false;
                ColliderTimer = OffTime;
            }
        }

    }
}