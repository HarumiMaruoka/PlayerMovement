using Game.Player.Movement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Debug
{
    public class Simulator : MonoBehaviour
    {
        private List<Recorder.Data> data;
        private PlayerMovement playerMovement = new PlayerMovement();
        private int index = 0;

        [SerializeField] private Button stepForwardButton;
        [SerializeField] private Button stepBackButton;

        [SerializeField] private Button stepForwardButton10;
        [SerializeField] private Button stepBackButton10;

        public MovementConfig config;

        private void Start()
        {
            data = Recorder.Load("recording.json");
            playerMovement.Start(config);
            stepForwardButton.onClick.AddListener(StepForward);
            stepBackButton.onClick.AddListener(StepBack);
            stepForwardButton10.onClick.AddListener(() => { for (int i = 0; i < 10; i++) { StepForward(); } });
            stepBackButton10.onClick.AddListener(() => { for (int i = 0; i < 10; i++) { StepBack(); } });
        }

        private void Update()
        {
            playerMovement.Update(0f, transform, default);
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