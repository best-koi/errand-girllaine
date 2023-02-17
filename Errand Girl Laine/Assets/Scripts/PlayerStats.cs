using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines the properties and actions related to the health stat.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private float maxHp;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHp;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            if (currentHealth <= 1)
            {
                Debug.Log("Game Over");
            }

            else
            {
                currentHealth -= 1;
                StartCoroutine(IFrames());
            }
        }
    }

    private void Heal()
    {
        if (currentHealth < maxHp && currentHealth > 0)
        currentHealth += 1;
    }

    IEnumerator IFrames()
    {
        Physics2D.IgnoreLayerCollision(7, 8);
        yield return new WaitForSeconds(2);
        Physics2D.IgnoreLayerCollision(7, 8, false);
    }

}
