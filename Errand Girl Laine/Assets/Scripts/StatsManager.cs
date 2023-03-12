using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    //public HealthUI healthBar;
    //private GameObject healthBarGO;

    //temporary
    public UnityEvent<float> healthChange; //for health bar

    [Header("Damage")]
    [SerializeField]
    private int damage;

    [Header("Block")]
    private bool isBlocking;

    [Header("I Frames")]
    [SerializeField]
    private float invSeconds;
    [SerializeField] private Canvas victory;
    private void Awake()
    {
        //healthBarGO = GameObject.Find("Health");
        //healthBar = healthBarGO.GetComponent<HealthUI>();
        currentHealth = maxHp;

        //healthBar.SetMaxHealth(maxHp);

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

    private void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.collider.tag == "AttackHitbox")
        {
            TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHp);
        healthChange?.Invoke((float) currentHealth / maxHp);
        Debug.Log("Took Damage");
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("Game Over");
            gameObject.SetActive(false);
            victory.gameObject.SetActive(true);
            //currentHealth -= damage;
            //healthBar.SetHealth(currentHealth);
        }
        else
        {
            StartCoroutine(IFrames());
        }
        
        // if (currentHealth >= 0)
        // {
        //     currentHealth -= damage;
        //     healthBar.SetHealth(currentHealth);
        //     Debug.Log("Took Damage");
        //     if (currentHealth <= 0)
        //     {
        //         Debug.Log("Game Over");
        //         gameObject.SetActive(false);
        //     }

        //     else
        //     {
        //         StartCoroutine(IFrames());
        //     }
        // }
    }

    public void Heal()
    {
        if (currentHealth < maxHp && currentHealth > 0)
        {
            currentHealth += healAmount;
            //healthBar.SetHealth(currentHealth);
        }
    }

    IEnumerator IFrames()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(invSeconds);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

}
