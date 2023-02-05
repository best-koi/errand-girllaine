using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Coded by Nancy
public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float speed = 200; //Speed can be changed in the inspector but is defaulted to 200
    Vector2 move;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Block(InputAction.CallbackContext context)
    {
        //if player is attacked/something hits the collider but is blocking, player takes less damage or no damage
    }

    void Attack(InputAction.CallbackContext context)
    {
        //player attacks and decreases health of other player
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(move.x * speed * Time.deltaTime, move.y * speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //if player moves left, look left, if player moves right, move right (still need to code)
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
