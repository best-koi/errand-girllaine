using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defines the properties and actions related to the health stat.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float attackDuration = 0.5f;
    private BoxCollider2D hitbox;

    //temporary
    public UnityEvent<float> healthChange; //for health bar

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    //Bryan's code
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.transform.tag == "Enemy")
    //    {
    //        if (currentHealth <= 1)
    //        {
    //            Debug.Log("Game Over");
    //        }

    //        else
    //        {
    //            currentHealth -= 1;
    //            StartCoroutine(IFrames());
    //        }
    //    }
    //}

    private void takeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        //healthChange?.Invoke((float)currentHealth / maxHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //player dies
            Destroy(gameObject);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            takeDamage(10);
        }
        StartCoroutine(IFrames());
    }

    IEnumerator IFrames()
    {
        yield return new WaitForSeconds(attackDuration);
    }
    
    private void Heal()
    {
        if (currentHealth < maxHealth && currentHealth > 0)
        currentHealth++;
    }

}
