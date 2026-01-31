using UnityEngine;
namespace Game.Player.Movement
{
    public class MovementContext
    {
        public Vector2 Position; // 現在の位置
        public Vector2 Velocity; // 現在の速度

        public bool IsGrounded;       // 接地しているか
        public Vector2? GroundNormal; // 接地面の法線。 nullの場合、接地していない
        public float? GroundAngle;    // 接地面の角度。 nullの場合、接地していない

        public float CoyoteTimer;               // コヨーテタイマー
        public float JumpHoldTimer;             // 可変ジャンプタイマー
        public float IgnoreOneWayPlatformTimer; // 片側通行プラットフォーム無視タイマー

#if UNITY_EDITOR
        internal Vector2 Delta;
        internal Vector2? LastMoveHitNormal; // nullの場合、衝突していない
#endif
    }
}