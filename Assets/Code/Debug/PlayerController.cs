using Game.Debug;
using Game.Player.Movement;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InputActions inputActions;
    public PlayerMovement playerMovement = new PlayerMovement();
    public MovementConfig movementConfig;
    public Recorder recorder = new Recorder();

    public const float DeltaTime = 1f / 60f;
    public bool isRecord = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        inputActions = new InputActions();

        inputActions.Enable();
        playerMovement.Start(movementConfig);
    }

    private void Update()
    {
        var input = ReadMovementInput();
        playerMovement.Update(DeltaTime, transform, input);
        if (isRecord) recorder.Record(new Recorder.Data() { Position = transform.position });
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

    public void OnDestroy()
    {
        if (isRecord)
        {
            recorder.Save("recording.json");
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
