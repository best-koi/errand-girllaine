using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    //temporary
    public UnityEvent<float> healthChange; //for health bar

    // Start is called before the first frame update
    private void Start()
    {
        currentHealth = maxHealth;
    }
    private void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.tag == "Player1")
        {
            takeDamage(10);
        }
    }

    private void takeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        healthChange?.Invoke((float) currentHealth / maxHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //enemy dies
            Destroy(gameObject);
        }
    }

    private void Attack()
    {
        //enemy attacks and decreases health of player
        //player.takeDamage(10);
    }


}
