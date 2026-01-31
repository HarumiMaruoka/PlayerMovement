using Game.Player.Movement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Debug
{
    public class Simulator : MonoBehaviour
    {
        private List<Recorder.Data> data;
        private PlayerMovement playerMovement = new PlayerMovement();
        private int index = 0;

        public MovementConfig config;

        private float repeatDelay = 0.1f;
        private float initialRepeatDelay = 1.0f;
        private float nextRepeatTime = 0f;
        private int repeatDirection = 0; // 1: forward, -1: back, 0: none

        private void Start()
        {
            data = Recorder.Load("recording.json");
            playerMovement.Start(config);
            var record = data[index];
            transform.position = (Vector3)record.Position;
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            HandleRepeatInput();

            playerMovement.Update(0f, transform, default);
        }

        private void HandleRepeatInput()
        {
            var keyboard = Keyboard.current;

            // 押し始め：即時に1回動かして、次は initialRepeatDelay 後
            if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                repeatDirection = 1;
                StepForward();
                nextRepeatTime = Time.unscaledTime + initialRepeatDelay;
                return;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                repeatDirection = -1;
                StepBack();
                nextRepeatTime = Time.unscaledTime + initialRepeatDelay;
                return;
            }

            // 離したら停止
            if (repeatDirection == 1 && keyboard.rightArrowKey.wasReleasedThisFrame)
            {
                repeatDirection = 0;
                return;
            }

            if (repeatDirection == -1 && keyboard.leftArrowKey.wasReleasedThisFrame)
            {
                repeatDirection = 0;
                return;
            }

            // 押しっぱなし repeat
            if (repeatDirection != 0)
            {
                var isHeld = repeatDirection == 1 ? keyboard.rightArrowKey.isPressed : keyboard.leftArrowKey.isPressed;
                if (!isHeld)
                {
                    repeatDirection = 0;
                    return;
                }

                if (Time.unscaledTime >= nextRepeatTime)
                {
                    if (repeatDirection == 1)
                    {
                        StepForward();
                    }
                    else
                    {
                        StepBack();
                    }

                    nextRepeatTime = Time.unscaledTime + repeatDelay;
                }
            }
        }

        public void StepForward()
        {
            if (data == null || index >= data.Count)
            {
                return;
            }
            index++;
            var record = data[index];
            transform.position = (Vector3)record.Position;
        }

        public void StepBack()
        {
            if (data == null || index <= 1)
            {
                return;
            }
            index--;
            var record = data[index];
            transform.position = (Vector3)record.Position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (playerMovement != null)
            {
                playerMovement.OnDrawGizmos(transform, config);
            }
        }

        private void OnGUI()
        {
            if (playerMovement != null)
            {
                playerMovement.OnGUI();
            }
        }
    }
#endif
}