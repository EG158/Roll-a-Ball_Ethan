/************************************************************
* COMPONENT OF: Player
* REQUIRED DEPENDENCIES: Rigidbody Component
* DESCRIPTION: It listens for WASD and Arrow key presses to set 
*              vertical and horizontal directions. It uses those
*              to push the player's Rigidbody in that direction
*              at a preset force.
* AUTHOR: Ethan Guo
* VERSION: 1.0
*************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Movement fields
    private float horizontalMovement;
    private float verticalMovement;
    private float force;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        force = 4.75f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlayer();
    }

    // Moves the player
    private void MovePlayer()
    {
        Vector3 direction = new Vector3(horizontalMovement, 0, verticalMovement);
        GetComponent<Rigidbody>().AddForce(direction * force);
    }  
        
    // Gets the user key input and uses it to assign movement directions
    private void SetMoveDirection(Vector2 input)
    {
        horizontalMovement = input.x;
        verticalMovement = input.y;
    }

    // Listens for WASD and arrow key input then calls SetMoveDirection
    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        SetMoveDirection(ctx.ReadValue<Vector2>());
    }

}
