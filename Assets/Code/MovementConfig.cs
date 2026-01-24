using UnityEngine;

namespace Game.Player.Movement
{
    public class MovementConfig : ScriptableObject
    {
        [Header("Collision Settings")]
        public float Skin = 0.02f;
        public Vector2 ColliderSize = new Vector2(0.9f, 1.8f);
        public Vector2 ColliderCenter = Vector2.zero;
        public float StepHeight = 0.2f;
        public float MaxGroundAngle = 45f;
        public float GroundRayLength = 0.1f;
        public Vector2 GroundProbeOffset = new Vector2(0f, -0.9f);
        public LayerMask GroundLayerMask;
        public LayerMask OneWayPlatformLayerMask;

        [Header("Movement Stats")]
        [Header("Walking")]
        public float WalkSpeed = 4f;
        public float WalkAcceleration = 20f;
        public float WalkDeceleration = 25f;
        public float WalkTurnSpeed = 30f;

        [Header("Dashing")]
        public float DashSpeed = 8f;
        public float DashAcceleration = 30f;
        public float DashDeceleration = 35f;
        public float DashTurnSpeed = 40f;

        [Header("Air Movement")]
        public float Gravity = 20f;
        public float AirMaxFallSpeed = 25f;
        public float AirControlSpeed = 100f;
        public float AirControlAcceleration = 10f;
        public float AirControlDeceleration = 15f;
        [Range(0f, 1f)]
        public float GravityReductionPercent; // ジャンプボタンホールド時の重力軽減率

        [Header("Times")]
        public float CoyoteTime;
        public float JumpBufferTime;
    }
}