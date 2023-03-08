using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Statmanager works for enemies and player

public class StatsManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private float maxHp;
    [SerializeField]
    private float currentHealth;
    [SerializeField]
    private float healAmount;
    public HealthUI healthBar;

    [Header("Damage")]
    [SerializeField]
    private float damage;

    [Header("Block")]
    private bool isBlocking;

    [Header("I Frames")]
    [SerializeField]
    private float invSeconds;

    private void Awake()
    {
        currentHealth = maxHp;
        healthBar.SetMaxHealth(maxHp);

    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        GetComponentInChildren
        if (collision.transform.tag == "Enemy")
        {
            if (currentHealth <= 0)
            {
                Debug.Log("Game Over");
            }

            else
            {
                currentHealth -= 1;
                Debug.Log("TookDamage");
                //StartCoroutine(IFrames());
            }
        }
    }
    */

    public void TakeDamage()
    {
        
        if (currentHealth <= 0)
        {
            Debug.Log("Game Over");
            Destroy(gameObject);
        }
        
        else
        {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth);
            Debug.Log("Took Damage");
            if (currentHealth <= 0)
            {
                Debug.Log("Game Over");
                gameObject.SetActive(false);
            }

            else
            {
                StartCoroutine(IFrames());
            }
        }
    }

    public void Heal()
    {
        if (currentHealth < maxHp && currentHealth > 0)
        {
            currentHealth += healAmount;
            healthBar.SetHealth(currentHealth);
        }
    }
    
    IEnumerator IFrames()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(invSeconds);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

}
