using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        // 接地判定
        private void Prove()
        {
            var hit = ProbeGroundCircleCast(_context.Position, out var groundProbeOrigin, out var groundProbeRadius,
                out var direction, out var groundProbeLength, out int layerMask);

            var angle = hit.collider != null ? Vector2.Angle(hit.normal, Vector2.up) : (float?)null;

            // 接地情報を更新
            // 接地条件:
            // 1. レイが何かにヒットしていて、その角度が最大接地角以下であること
            // 2. ヒットしたオブジェクトが片側通行プラットフォームである場合、無視タイマーが0以下であること
            // 3. ジャンプ入力猶予タイマーが0以下であること（ジャンプ直後は接地とみなさない）

            var isGroundAngleOk = angle.HasValue && angle.Value <= _config.MaxGroundAngle;
            var isOneWayPlatform = hit.collider != null && ((_config.OneWayPlatformLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0);
            var isOneWayPlatformOk = !isOneWayPlatform || _context.IgnoreOneWayPlatformTimer <= 0f;
            var isJumpBufferOk = _context.JumpHoldTimer <= 0f;
            var wasGrounded = _context.IsGrounded;

            _context.IsGrounded = isGroundAngleOk && isOneWayPlatformOk && isJumpBufferOk;
            _context.GroundNormal = _context.IsGrounded ? hit.normal : null;
            _context.GroundAngle = _context.IsGrounded ? angle : null;

            // 離陸時にコヨーテタイマーをリセット
            if (wasGrounded && !_context.IsGrounded)
            {
                _context.CoyoteTimer = _config.CoyoteTime;
            }
        }

        // Gizmoでも使えるようにするために分離
        private RaycastHit2D ProbeGroundCircleCast(Vector2 position, out Vector2 origin, out float radius, out Vector2 direction, out float length, out int layerMask)
        {
            var colliderSize = _config.ColliderSize;
            var colliderCenter = position + _config.ColliderOffset;
            origin = colliderCenter + new Vector2(0f, -colliderSize.y / 2f) + _config.GroundProbeOffset;
            length = _config.GroundRayLength + _config.Skin;
            radius = colliderSize.x / 2f;
            direction = Vector2.down;
            layerMask = _config.GroundLayerMask | _config.OneWayPlatformLayerMask;
            return Physics2D.CircleCast(origin, radius, direction, length, layerMask);
        }
    }
}