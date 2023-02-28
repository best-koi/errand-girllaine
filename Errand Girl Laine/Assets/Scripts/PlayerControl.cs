using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

//Coded by Nancy
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float speed = 20; //Speed can be changed in the inspector but is defaulted to 20
    private Vector2 move;
    private Rigidbody2D rb;
    private int facingDirection = 1; //Checks which direction the player is facing, is right by default
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


    public void Block(InputAction.CallbackContext context)
    {
        //if player is attacked/something hits the collider but is blocking, player takes less damage or no damage
        //if key pressed switch to block animation
        animator.SetTrigger("Block"); //sets "block" trigger to on to play the block animation
    }

    public void Attack(InputAction.CallbackContext context)
    {
        //player attacks and decreases health of other player
        animator.SetTrigger("Attack"); //sets "attack" trigger to on to play the attack animation
    }


    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + (move * speed * Time.deltaTime));
    }

    // Update is called once per frame
    private void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        checkDirection();
        
    }

    private void checkDirection()
    {
        float inputRaw = Input.GetAxisRaw("Horizontal"); //if player moves left, look left, if player moves right, look right

        if (inputRaw > 0) //looks right
        {
            //GetComponent<SpriteRenderer>().flipX = false;
            if (facingDirection == -1) FlipPlayer();
            facingDirection = 1;
            animator.SetBool("LookLeft", false);
        }

        else if (inputRaw < 0) //looks left
        {
            //FlipPlayer();
            //GetComponent<SpriteRenderer>().flipX = true;
            if (facingDirection == 1) FlipPlayer();
            facingDirection = -1;
            animator.SetBool("LookLeft", true);
        } 
    }

    private void FlipPlayer()
    {
        Vector3 oppDirection = transform.localScale;
        oppDirection.x *= -1;
        transform.localScale = oppDirection;
    }

}
