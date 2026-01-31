using Game.Debug;
using Game.Player.Movement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_Step : MonoBehaviour
{
    public PlayerMovement playerMovement = new PlayerMovement();
    public MovementConfig movementConfig;

    public Toggle leftToggle;
    public Toggle rightToggle;
    public Toggle jumpToggle;
    public Button stepButton;

    public const float DeltaTime = 1f / 60f;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        playerMovement.Start(movementConfig);

        stepButton.onClick.AddListener(() =>
        {
            var input = ReadMovementInput();
            playerMovement.Update(DeltaTime, transform, input);
        });
    }

    private MovementInput ReadMovementInput()
    {
        var result = new MovementInput();
        if (leftToggle.isOn)
        {
            result.Move.x = -1f;
        }
        else if (rightToggle.isOn)
        {
            result.Move.x = 1f;
        }
        result.Jump = jumpToggle.isOn;
        return result;
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
}
