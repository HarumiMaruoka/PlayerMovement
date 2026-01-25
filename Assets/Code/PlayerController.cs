using Game.Player.Movement;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InputActions inputActions;
    public PlayerMovement playerMovement = new PlayerMovement();
    public MovementConfig movementConfig;

    private void Awake()
    {
        inputActions = new InputActions();

        inputActions.Enable();
        playerMovement.Start(movementConfig);
    }

    private void Update()
    {
        playerMovement.Update(Time.deltaTime, transform, ReadMovementInput());
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (playerMovement != null)
        {
            playerMovement.OnGUI();
        }
    }

    private void OnDrawGizmos()
    {
        if (playerMovement != null)
        {
            playerMovement.OnDrawGizmos(transform, movementConfig);
        }
    }
#endif

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
