using Game.Player.Movement;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InputActions inputActions;
    public PlayerMovement playerMovement;

    private void Awake()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        playerMovement = new PlayerMovement();
        playerMovement.Start();
    }

    private void Update()
    {
        playerMovement.Update(ReadMovementInput());
    }

    private MovementInput ReadMovementInput()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        bool jump = inputActions.Player.Jump.IsPressed();
        bool sprint = inputActions.Player.Sprint.IsPressed();
        bool drop = move.y < -0.5f && inputActions.Player.Jump.triggered;


        return new MovementInput
        {
            Move = move,
            Jump = jump,
            Sprint = sprint,
            Drop = drop
        };
    }
}
