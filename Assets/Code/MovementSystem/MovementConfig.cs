using UnityEngine;

namespace Game.Player.Movement
{
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Game/Player/MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        [Header("Collision Settings")]
        public float Skin = 0.02f;
        public Vector2 ColliderSize = new Vector2(0.9f, 1.8f);
        public Vector2 ColliderOffset = new Vector2(0f, 0.9f);
        public float StepHeight = 0.2f;
        public float MaxGroundAngle = 45f;
        public float GroundRayLength = 0.1f;
        public float GroundSnapRayLength = 0.3f;
        public Vector2 GroundProbeOffset = new Vector2(0f, -0.9f);
        public LayerMask GroundLayerMask;
        public LayerMask OneWayPlatformLayerMask;

        [Header("Movement Stats")]
        [Header("Walking")]
        public float WalkMaxSpeed = 4f;
        public float WalkAcceleration = 20f;
        public float WalkDeceleration = 25f;
        public float WalkTurnSpeed = 30f;

        [Header("Sprinting")]
        public float SprintMaxSpeed = 8f;
        public float SprintAcceleration = 30f;
        public float SprintDeceleration = 35f;
        public float SprintTurnSpeed = 40f;

        [Header("Jumping")]
        public float JumpForce = 10f;

        [Header("Air Movement")]
        public float Gravity = 20f;
        public float AirMaxFallSpeed = 25f;
        public float AirHorizontalMaxSpeed = 100f;
        public float AirControlAcceleration = 10f;
        public float AirControlDeceleration = 15f;
        public float AirTurnSpeed = 15f;
        [Range(0f, 1f)]
        public float JumpBufferGravityMultiplier; // ジャンプボタンホールド時の重力軽減率

        [Header("Times")]
        public float CoyoteTime = 0.1f;
        public float JumpHoldTimeRemaining = 0.2f;

#if UNITY_EDITOR
        [Header("Debug")]
        public bool ShowGizmos = true;
        public bool ShowGroundProbeGizmos = true;
        public bool ShowMovementCapsuleCastGizmos = true;
        public bool ShowStepUpCapsuleCastGizmos = true;
        public bool ShowGroundSnapCapsuleCastGizmos = true;
#endif
    }
}