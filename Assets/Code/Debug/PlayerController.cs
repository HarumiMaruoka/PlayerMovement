using Game.Player.Movement;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerMovement playerMovement = new PlayerMovement();
        private InputSystem_Actions inputActions;

        public const float DeltaTime = 60f / 1f;
        public MovementConfig config;

        public bool IsStepMode = false;
        private float repeatDelay = 0.1f;
        private float initialRepeatDelay = 1.0f;
        private float nextRepeatTime = 0.0f;

        private bool isInitialized = false;

        void Start()
        {
            if (!config)
            {
                throw new UnassignedReferenceException("MovementConfig が設定されていません。Inspector で割り当ててください。");
            }

            playerMovement.Start(config);
            inputActions = new InputSystem_Actions();
            isInitialized = true;
        }

        void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            if (Keyboard.current == null || Mouse.current == null)
            {
                UnityEngine.Debug.LogWarning($"キーボードまたはマウスが検出されませんでした。{{ Keyboard: {Keyboard.current == null}, Mouse: {Mouse.current == null} }}");
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                IsStepMode = !IsStepMode;
            }

            if (!IsStepMode)
            {
                playerMovement.Update(DeltaTime, transform, ReadInput());
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                playerMovement.Update(DeltaTime, transform, ReadInput());
                nextRepeatTime = initialRepeatDelay;
            }
            else if (Mouse.current.leftButton.IsPressed())
            {
                nextRepeatTime -= Time.unscaledDeltaTime;

                if (nextRepeatTime < 0f)
                {
                    playerMovement.Update(DeltaTime, transform, ReadInput());
                    nextRepeatTime = repeatDelay;
                }
            }
        }

        private void OnGUI()
        {
            if (playerMovement != null)
            {
                playerMovement.DebugParams.IsStepMode = IsStepMode;
                playerMovement.OnGUI();
            }
        }

        private void OnDrawGizmos()
        {
            if (playerMovement != null && transform && config)
            {
                playerMovement.OnDrawGizmos(transform, config);
            }
        }

        MovementInput ReadInput()
        {
            Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
            bool jump = inputActions.Player.Jump.IsPressed();
            bool sprint = inputActions.Player.Sprint.IsPressed();
            bool drop = move.y < -0.5f && inputActions.Player.Jump.triggered;

            return new MovementInput() { };
        }
    }
}