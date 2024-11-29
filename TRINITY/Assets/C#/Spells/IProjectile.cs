using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float Duration { get; set; }
    public float MaxRange { get; set; }
    public float Cooldown { get; set; }
}
