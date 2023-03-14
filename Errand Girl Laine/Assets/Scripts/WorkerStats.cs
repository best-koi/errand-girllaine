using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerStats : StatsManager
{
    protected override void Awake()
    {
        currentHealth = maxHp;
    }
    protected override void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.collider.tag == "AttackHitbox")
        {
            TakeDamage(damage);
        }
    }
}
