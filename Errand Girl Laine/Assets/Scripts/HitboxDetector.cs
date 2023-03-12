using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxDetector : MonoBehaviour
{
    [SerializeField]
    private StatsManager sm;
    [SerializeField]


    private void Awake()
    {
       sm = GetComponent<StatsManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.collider.tag);
        if (collision.gameObject.tag == "Damage Hitbox")
        {
            Debug.Log("Hit");
            //sm.TakeDamage();
        }
    }
}
