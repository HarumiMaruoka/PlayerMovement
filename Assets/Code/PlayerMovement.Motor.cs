using System;
using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        // 移動、衝突解決
        private void ApplyMovement(float deltaTime)
        {
            if (_context.Velocity == Vector2.zero)
            {
                return;
            }

            if (_context.IsGrounded)
            {
                ApplyGroundMovement(deltaTime);
            }
            else
            {
                ApplyAirMovement(deltaTime);
            }
        }

        private void ApplyGroundMovement(float deltaTime)
        {
            var position = _context.Position;
            var delta = _context.Velocity * deltaTime;
            var groundNormal = _context.GroundNormal.Value;
            var groundTangent = new Vector2(groundNormal.y, -groundNormal.x);
            var stepUpHeight = _config.StepHeight;
            delta = groundTangent * Vector2.Dot(delta, groundTangent);

            var rayOrigin = _context.Position + _config.ColliderOffset;
            var raySize = _config.ColliderSize;
            var capsuleDirection = CapsuleDirection2D.Vertical;
            var rayAngle = 0f;
            var rayDirection = delta.normalized;
            var rayLength = delta.magnitude + _config.Skin;
            var layerMask = _config.GroundLayerMask;

            var hit = Physics2D.CapsuleCast(rayOrigin, raySize, capsuleDirection, rayAngle, rayDirection, rayLength, layerMask);

            if (hit.collider == null)
            {
                // 衝突なし
                position += delta;
            }
            else
            {
                // 衝突あり
                // ステップアップ処理
                if (stepUpHeight > 0f)
                {
                    var stepUpOrigin = rayOrigin + new Vector2(0f, stepUpHeight);
                    var stepUpHit = Physics2D.CapsuleCast(stepUpOrigin, raySize, capsuleDirection, rayAngle, rayDirection, rayLength, layerMask);
                    if (stepUpHit.collider == null)
                    {
                        // ステップアップ成功
                        position += new Vector2(0f, stepUpHeight);
                        position += delta;
                    }
                    else
                    {
                        // ステップアップ失敗、通常の衝突処理へ
                        // 衝突点まで移動
                        var distanceToHit = hit.distance - _config.Skin;
                        position += delta.normalized * distanceToHit;
                    }
                }
            }

            // 地面吸着処理
            // 下に向かってレイキャストを飛ばし、地面に吸着させる。長さはステップ高さ分。
            var groundSnapOrigin = position + _config.ColliderOffset + new Vector2(0f, stepUpHeight);
            var groundSnapHit = Physics2D.CapsuleCast(groundSnapOrigin, raySize, capsuleDirection, rayAngle, Vector2.down, stepUpHeight + _config.Skin, layerMask);
            if (groundSnapHit.collider != null)
            {
                var distanceToGround = groundSnapHit.distance - _config.Skin;
                position += Vector2.down * distanceToGround;
            }

            _context.Position = position;
        }

        private void ApplyAirMovement(float deltaTime)
        {
            // 空中移動
            var position = _context.Position;
            var delta = _context.Velocity * deltaTime;

            // レイキャストで衝突判定
            var rayOrigin = _context.Position + _config.ColliderOffset;
            var raySize = _config.ColliderSize;
            var capsuleDirection = CapsuleDirection2D.Vertical;
            var rayAngle = 0f;
            var rayDirection = delta.normalized;
            var rayLength = delta.magnitude + _config.Skin;
            var layerMask = _config.GroundLayerMask;
            var hit = Physics2D.CapsuleCast(rayOrigin, raySize, capsuleDirection, rayAngle, rayDirection, rayLength, layerMask);
            if (hit.collider == null)
            {
                // 衝突なし
                position += delta;
            }
            else
            {
                // 衝突あり
                // 衝突点まで移動
                var distanceToHit = hit.distance - _config.Skin;
                position -= delta.normalized * distanceToHit;
            }

            _context.Position = position;
        }
    }
}