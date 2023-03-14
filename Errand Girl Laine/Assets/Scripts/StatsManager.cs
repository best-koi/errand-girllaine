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
    private Animator animator;

    [Header("Health")]
    [SerializeField]
    protected float maxHp;
    [SerializeField]
    protected float currentHealth;
    [SerializeField]
    private float healAmount;

    //public HealthUI healthBar;
    //private GameObject healthBarGO;

    //temporary
    public UnityEvent<float> healthChange; //for health bar

    //Test
    public PlayerControl pc;

    [Header("Damage")]
    [SerializeField]
    protected int damage;

    [Header("Block")]
    public bool blocking;

    [Header("I Frames")]
    [SerializeField]
    private float invSeconds;
    [SerializeField] private Canvas victory;
    protected virtual void Awake()
    {
        //healthBarGO = GameObject.Find("Health");
        //healthBar = healthBarGO.GetComponent<HealthUI>();
        currentHealth = maxHp;
        animator = GetComponent<Animator>();
        pc = GetComponent<PlayerControl>();
        blocking = animator.GetBool("Block");
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

    protected virtual void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.collider.tag == "AttackHitbox")
        {
            blocking = animator.GetBool("Block");
            Debug.Log("JACKFLAP");
            if (blocking == true)
            {
                animator.Play("LaineSuccessfulBlock");
                Debug.Log("Blocked");
            }

            else
            {
                TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHp);
        healthChange?.Invoke((float) currentHealth / maxHp);
        Debug.Log("Took Damage");
        if (currentHealth <= 0)
        {
            Debug.Log("Game Over");
            gameObject.SetActive(false);
            if(this.tag == "Enemy") victory.gameObject.SetActive(true);
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
