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
    public Animator animator;

    /* public void CheckPlayer()
    {
        //var player1 = PlayerInput.Instantiate(playerPrefab, controlScheme: "Player1", pairWithDevice: Keyboard.current);
        //var player2 = PlayerInput.Instantiate(playerPrefab, controlScheme: "Player2", pairWithDevice: Keyboard.current);

        if (gameObject.tag == "Player1")
        {
            //if player has Player1 tag, only WASD keys are enabled
        }
        
        else if (gameObject.tag == "Player2")
        {
            //if player has Player2 tag, only arrow keys are enabled
        }
    } */


    //if block is successful, transition to SuccessfulBlock ((NEEDS TO CHECK FOR COLLISION))
    public void Block(InputAction.CallbackContext context)
    {
        //if player is attacked/something hits the collider but is blocking, player takes less damage or no damage
        //if key pressed switch to block animation
        if(Input.GetKeyDown(KeyCode.Q)) 
        {
            animator.SetBool("Block", true); //sets "block" trigger to on to play the block animation
            animator.Play("LaineBlock");
            //while (Input.GetKey(KeyCode.Q)) animator.Play("LaineBlockFrame");
        }

        else if (Input.GetKeyUp(KeyCode.Q))
        {
            animator.SetBool("Block", false);
        }
        
    }

    public void Attack(InputAction.CallbackContext context)
    {
        //player attacks and decreases health of other player
        if(Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Attack"); //sets "attack" trigger to on to play the attack animation
        }
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
            if (facingDirection == -1) FlipPlayer();
            facingDirection = 1;
            animator.SetBool("LookLeft", false);
        }

        else if (inputRaw < 0) //looks left
        {
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
