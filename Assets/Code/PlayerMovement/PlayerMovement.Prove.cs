using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        // 接地判定
        private void GroundProve()
        {
            var currentPosition = _context.Position;
            var boxRayOrigin = currentPosition + _config.ColliderOffset + _config.GroundProbeOffset;
            var boxRaySize = _config.GroundProveSize;
            var rayAngle = 0f;
            var rayLength = _config.GroundRayLength;
            var rayDirection = Vector2.down;
            var layerMask = _config.GroundLayerMask | _config.OneWayPlatformLayerMask;

            var groundHit = Physics2D.BoxCast(boxRayOrigin, boxRaySize, rayAngle, rayDirection, rayLength, layerMask);
            var angle = groundHit.collider != null ? Vector2.Angle(groundHit.normal, Vector2.up) : (float?)null;

            var isHit = groundHit.collider != null && groundHit.distance > 0;
            var isGroundAngleOk = angle.HasValue && angle.Value <= _config.MaxGroundAngle;
            var isOneWayPlatform = groundHit.collider != null && ((_config.OneWayPlatformLayerMask.value & (1 << groundHit.collider.gameObject.layer)) != 0);
            var isOneWayPlatformOk = !isOneWayPlatform || _context.IgnoreOneWayPlatformTimer <= 0f;
            var isJumpBufferOk = _context.JumpHoldTimer <= 0f;
            var wasGrounded = _context.IsGrounded;

            _context.IsGrounded = isGroundAngleOk && isOneWayPlatformOk && isJumpBufferOk;
            _context.GroundNormal = _context.IsGrounded ? groundHit.normal : null;
            _context.GroundAngle = _context.IsGrounded ? angle : null;

            // 離陸時にコヨーテタイマーをリセット
            if (wasGrounded && !_context.IsGrounded)
            {
                _context.CoyoteTimer = _config.CoyoteTime;
            }
        }
    }
}