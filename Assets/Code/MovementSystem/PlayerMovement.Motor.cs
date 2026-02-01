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

            if (hit.collider == null || hit.distance <= 0f)
            {
                // 衝突なし
                position += delta;
            }
            else
            {
                // 衝突あり
                // ステップアップ処理
                var stepUpOrigin = rayOrigin + new Vector2(0f, stepUpHeight);
                var stepUpHit = Physics2D.CapsuleCast(stepUpOrigin, raySize, capsuleDirection, rayAngle, rayDirection, rayLength, layerMask);
                if (stepUpHit.collider == null || stepUpHit.distance <= 0f)
                {
                    // ステップアップ成功
                    var stepUpDistanceToHit = Mathf.Max(0f, hit.distance - _config.Skin);
                    var stepUpDelta = delta + new Vector2(0f, stepUpHeight);
                    var stepUpLength = stepUpDelta.magnitude + _config.Skin;
                    position += stepUpDelta.normalized * stepUpLength;
                }
                else
                {
                    // ステップアップ失敗、通常の衝突処理へ
                    // 衝突点まで移動
                    var distanceToHit = Mathf.Max(0f, hit.distance - _config.Skin);
                    position += delta.normalized * distanceToHit;
                }
            }

            // 地面吸着処理
            // 下に向かってレイキャストを飛ばし、地面に吸着させる。長さはステップ高さ分。
            var groundSnapOrigin = position + _config.ColliderOffset;
            var groundSnapHit = Physics2D.CapsuleCast(groundSnapOrigin, raySize, capsuleDirection, rayAngle, Vector2.down, _config.GroundSnapRayLength + _config.Skin, layerMask);
            if (groundSnapHit.collider != null && groundSnapHit.distance > 0f)
            {
                var distanceToGround = Mathf.Max(0f, groundSnapHit.distance - _config.Skin);
                position += Vector2.down * distanceToGround;

                // 移動先が急こう配の上り坂の場合（下り坂ならするりと降りられるようにする）
                var groundAngle = Vector2.Angle(groundSnapHit.normal, Vector2.up);
                if (groundAngle > _config.MaxGroundAngle)
                {
                    // 移動方向が上り坂方向かどうかを判定
                    var movementDir = delta.sqrMagnitude > 0f ? delta.normalized : Vector2.zero;

                    var n = groundSnapHit.normal;
                    var slopeTangent = new Vector2(n.y, -n.x).normalized; // 斜面接線（どちら向きかは未確定）
                    if (Vector2.Dot(slopeTangent, Vector2.up) < 0f)
                    {
                        slopeTangent = -slopeTangent; // 上り方向に揃える
                    }

                    var isTryingToGoUphill = Vector2.Dot(movementDir, slopeTangent) > 0f;

                    // 急こう配の上り坂を登ろうとする場合、坂方向にレイキャストを飛ばして衝突判定を行い、衝突点まで移動させる。
                    var uphillHit = Physics2D.CapsuleCast(rayOrigin, raySize, capsuleDirection, rayAngle, rayDirection, rayLength, layerMask);
                    if (isTryingToGoUphill && uphillHit.collider != null && uphillHit.distance > 0f)
                    {
                        var uphillDistanceToHit = Mathf.Max(0f, uphillHit.distance - _config.Skin);
                        position = _context.Position; // リセット
                        position += delta.normalized * uphillDistanceToHit;
                        _context.Velocity = Vector2.zero; // 衝突したら速度をゼロにする
                    }
                }
            }

            _context.Position = position;

#if UNITY_EDITOR
            _context.Delta = delta;
            _context.LastMoveHitNormal = hit.collider != null ? hit.normal : null;
#endif
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
            if (hit.collider == null || hit.distance <= 0f)
            {
                // 衝突なし
                position += delta;
            }
            else
            {
                // 衝突あり：衝突点直前まで進めて、残りを衝突面に沿ってスライドさせる
                var moveToHitDistance = Mathf.Max(0f, hit.distance - _config.Skin - 0.01f);
                var moveToHit = rayDirection * moveToHitDistance;
                position += moveToHit;

                var remainingDelta = delta - moveToHit;

                // remainingDelta を衝突面に沿う成分に投影
                var collisionNormal = hit.normal;
                var slideDelta = (Vector2)Vector3.ProjectOnPlane(remainingDelta, collisionNormal);

                // 再度レイキャストで衝突判定
                rayOrigin = position + _config.ColliderOffset;
                rayDirection = slideDelta.normalized;
                rayLength = slideDelta.magnitude + _config.Skin;
                var slideHit = Physics2D.CapsuleCast(rayOrigin, raySize, capsuleDirection, rayAngle, rayDirection, rayLength, layerMask);
                if (slideHit.collider == null || slideHit.distance <= 0f)
                {
                    // 衝突なし
                    position += slideDelta;
                }
                else
                {
                    // 衝突あり：衝突点直前まで進めるのみ
                    var slideToHitDistance = Mathf.Max(0f, slideHit.distance - _config.Skin);
                    var slideToHit = rayDirection * slideToHitDistance;
                    position += slideToHit;
                }
            }

            _context.Position = position;


#if UNITY_EDITOR
            _context.Delta = delta;
            _context.LastMoveHitNormal = hit.collider != null ? hit.normal : null;
#endif
        }
    }
}