using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Coded by Nancy
public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float speed = 20; //Speed can be changed in the inspector but is defaulted to 20
    private Vector2 move;
    private Rigidbody2D rb;
    private int m_facingDirection; //Checks which direction the player is facing
    private Animator animator;

    /*void CheckPlayer()
    {
        //if player has Player1 tag, only WASD controls are enabled
        if (gameObject.tag == "Player1")
        {

        }
        //if player has Player2 tag, only arrow keys are enabled
        else if (gameObject.tag == "Player2")
        {

        }
    } */


    void Block(InputAction.CallbackContext context)
    {
        //if player is attacked/something hits the collider but is blocking, player takes less damage or no damage
        //if key pressed switch to block animation
    }

    void Attack(InputAction.CallbackContext context)
    {
        //player attacks and decreases health of other player
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    public void FixedUpdate()
    {
        rb.MovePosition(rb.position + (move * speed * Time.deltaTime));
    }

    // Update is called once per frame
    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float inputRaw = Input.GetAxisRaw("Horizontal"); //if player moves left, look left, if player moves right, look right
        //looks right
        if (inputRaw > 0) 
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        //looks left
        else if (inputRaw < 0) 
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        if(rb != null)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {

            }
        }
    }

    /* I took this from an older script I made, but this affects the other player's health as opposed to the current player
     * will have to modify
     * private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player2")
        {
            //decreases health of Player2
        } 
    }
    */
}
