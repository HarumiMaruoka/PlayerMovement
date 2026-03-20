using System;
using UnityEngine;

namespace Confront.Player.Movement
{
    public partial class PlayerMovement
    {
        // 移動、衝突解決
        private void ApplyMovement(float deltaTime)
        {
            if (_context.IsGrounded)
            {
                GroundMove(deltaTime);
            }
            else
            {
                AirborneMove(deltaTime);
            }
        }

        private void GroundMove(float deltaTime)
        {
            DebugParams.DebugPoints.Clear();

            var position = _context.Position;
            var moveDelta = _context.Velocity * deltaTime;
            var moveSign = Mathf.Sign(moveDelta.x);

            if (Mathf.Approximately(moveDelta.x, 0f))
            {
                _context.Position = position;
                return;
            }

            var rayOrigin = _context.Position + new Vector2((_config.ColliderSize.x / 2f) * moveSign, -_config.ColliderSize.y / 2f + _config.StepHeight); // プレーヤーの矩形の右下、左下の頂点。（移動方向に合わせて右か左が決まる。）
            var rayDirection = Vector2.down;
            var rayDistance = _config.StepHeight + _config.GroundSnapRayLength + _config.Skin;
            var layerMask = _config.GroundLayerMask | _config.OneWayPlatformLayerMask;

            bool IsMovable(RaycastHit2D moveHit, bool isOriginEmbedded)
            {
                if (isOriginEmbedded)
                {
                    return false;
                }

                if (moveHit.collider == null)
                {
                    return true;
                }

                if (moveHit.distance <= 0f)
                {
                    return false;
                }

                var angle = Vector2.Angle(moveHit.normal, Vector2.up);
                if (angle > _config.MaxGroundAngle)
                {
                    // 下り坂の場合は移動を許可する
                    // 法線の水平成分が移動方向と同じ向き → 下り坂
                    var isDownhill = moveHit.normal.x * moveSign > 0f;
                    if (!isDownhill)
                    {
                        return false;
                    }
                }

                var isOneWayPlatform = (_config.OneWayPlatformLayerMask.value & (1 << moveHit.collider.gameObject.layer)) != 0;
                if (isOneWayPlatform && _context.IgnoreOneWayPlatformTimer > 0f)
                {
                    return true;
                }

                return true;
            }

            void MoveByBoxCast(ref Vector2 currentPosition, Vector2 delta, LayerMask castLayerMask)
            {
                if (delta == Vector2.zero)
                {
                    return;
                }

                var castDirection = delta.normalized;
                var castDistance = delta.magnitude + _config.Skin;
                var castHit = Physics2D.BoxCast(currentPosition + _config.ColliderOffset, _config.ColliderSize, 0f, castDirection, castDistance, castLayerMask);

                if (castHit.collider == null || castHit.distance <= 0f)
                {
                    currentPosition += delta;
                }
                else
                {
                    var moveDistance = Mathf.Max(0f, castHit.distance - _config.Skin);
                    currentPosition += castDirection * moveDistance;
                }
            }

#if UNITY_EDITOR
            void AddRayDebug(Vector2 origin, RaycastHit2D hit, Color beginColor, Color endColor)
            {
                var isHit = hit.collider != null && hit.distance > 0f;
                var endDistance = isHit ? hit.distance : rayDistance;
                var endPoint = origin + rayDirection * endDistance;

                DebugParams.DebugPoints.Add(new DebugPointInfo(origin, beginColor));
                DebugParams.DebugPoints.Add(new DebugPointInfo(endPoint, endColor));
            }
#endif

            // 二分探索的に移動可能な座標かどうかの境界を探し、移動不能な座標を見つけたらSkin分手前で停止する。
            // rayの開始地点と、終了地点を DebugParams.DebugPoints に追加する。

            var isRayOriginEmbedded = Physics2D.OverlapPoint(rayOrigin, layerMask) != null;
            var hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, layerMask);
            var isHitMovable = IsMovable(hit, isRayOriginEmbedded);

#if UNITY_EDITOR
            AddRayDebug(rayOrigin, hit, Color.yellow, isHitMovable ? Color.green : Color.red);
#endif

            var minDistance = 0f;
            var maxDistance = Mathf.Abs(moveDelta.x) + _config.Skin;
            var bestDistance = 0f;
            var bestHit = new RaycastHit2D();

            if (isHitMovable)
            {
                bestHit = hit;
                bestDistance = 0f;
            }

            const int iterationCount = 8;
            for (var i = 0; i < iterationCount; i++)
            {
                var checkDistance = (minDistance + maxDistance) * 0.5f;
                var checkPosition = position + new Vector2(checkDistance * moveSign, 0f);
                var checkRayOrigin = checkPosition + new Vector2((_config.ColliderSize.x / 2f) * moveSign, -_config.ColliderSize.y / 2f + _config.StepHeight);
                var isCheckRayOriginEmbedded = Physics2D.OverlapPoint(checkRayOrigin, layerMask) != null;
                var checkHit = Physics2D.Raycast(checkRayOrigin, rayDirection, rayDistance, layerMask);
                var isCheckMovable = IsMovable(checkHit, isCheckRayOriginEmbedded);

#if UNITY_EDITOR
                AddRayDebug(checkRayOrigin, checkHit, Color.yellow, isCheckMovable ? Color.green : Color.red);
#endif

                if (isCheckMovable && bestDistance < checkDistance)
                {
                    bestDistance = checkDistance;
                    bestHit = checkHit;
                    minDistance = checkDistance;
                }
                else
                {
                    maxDistance = checkDistance;
                }
            }

            if (bestDistance > 0f)
            {
                bestDistance = Mathf.Max(0f, bestDistance - _config.Skin);
                var targetPosition = position;

                if (bestHit.collider != null && bestHit.distance > 0f)
                {
                    var adjustRayOrigin = targetPosition + new Vector2((_config.ColliderSize.x / 2f) * moveSign, -_config.ColliderSize.y / 2f + _config.StepHeight);
                    var groundPointY = adjustRayOrigin.y - bestHit.distance;
                    targetPosition.x += bestDistance * moveSign;
                    targetPosition.y = groundPointY + (_config.ColliderSize.y / 2f) + _config.Skin;
                }
                else
                {
                    var groundNormal = _context.GroundNormal.Value;
                    var groundTangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;

                    if (!Mathf.Approximately(groundTangent.x, 0f) && Mathf.Sign(groundTangent.x) != moveSign)
                    {
                        groundTangent = -groundTangent;
                    }

                    var delta = Mathf.Approximately(groundTangent.x, 0f)
                        ? new Vector2(moveDelta.x, 0f)
                        : groundTangent * (moveDelta.x / groundTangent.x);
                    targetPosition = position + delta;
                }

                DebugParams.Delta = targetPosition - position;

                var resolvedPosition = position;

                // Step1: StepHeight分上に移動してエッジを乗り越える余裕を作る
                MoveByBoxCast(ref resolvedPosition, new Vector2(0f, _config.StepHeight), layerMask);

                // Step2: 横方向の移動
                MoveByBoxCast(ref resolvedPosition, new Vector2(targetPosition.x - resolvedPosition.x, 0f), layerMask);

                // Step3: ターゲット位置のYまで下降して地面にスナップ
                MoveByBoxCast(ref resolvedPosition, new Vector2(0f, targetPosition.y - resolvedPosition.y), layerMask);

                position = resolvedPosition;
            }

            _context.Position = position;
        }

        private void AirborneMove(float deltaTime)
        {
            // 縦の移動を先に解決する。
            // 縦の移動解決中に衝突した場合は、衝突点の法線に沿って残りの移動量を移動させる。
            // そのあとに横の移動を解決する。横の移動解決中に衝突した場合は、その地点で移動を終了する。

            var position = _context.Position;
            var moveDelta = _context.Velocity * deltaTime;
            var layerMask = _config.GroundLayerMask;
            LayerMask downLayerMask = (_context.IgnoreOneWayPlatformTimer <= 0f)
                ? (_config.GroundLayerMask | _config.OneWayPlatformLayerMask)
                : _config.GroundLayerMask;

            // 縦の移動を先に解決する。
            if (!Mathf.Approximately(moveDelta.y, 0f))
            {
                var direction = new Vector2(0f, Mathf.Sign(moveDelta.y));
                var distance = Mathf.Abs(moveDelta.y) + _config.Skin;
                var castLayerMask = moveDelta.y < 0f ? downLayerMask : layerMask;

                var hit = Physics2D.BoxCast(position + _config.ColliderOffset, _config.ColliderSize, 0f, direction, distance, castLayerMask);

                if (hit.collider != null && hit.distance > 0f)
                {
                    var moveDistance = Mathf.Max(0f, hit.distance - _config.Skin);
                    position += direction * moveDistance;

                    // 衝突点の法線に沿って残りの移動量を移動させる
                    var remaining = direction * (Mathf.Abs(moveDelta.y) - moveDistance);
                    var slide = remaining - Vector2.Dot(remaining, hit.normal) * hit.normal;

                    if (slide.sqrMagnitude > 0.001f * 0.001f)
                    {
                        var slideHit = Physics2D.BoxCast(position + _config.ColliderOffset, _config.ColliderSize, 0f, slide.normalized, slide.magnitude + _config.Skin, castLayerMask);

                        if (slideHit.collider == null || slideHit.distance <= 0f)
                        {
                            position += slide;
                        }
                        else
                        {
                            var slideMoveDistance = Mathf.Max(0f, slideHit.distance - _config.Skin);
                            position += slide.normalized * slideMoveDistance;
                        }
                    }

                    if (_context.Velocity.y > 0f)
                    {
                        _context.Velocity.y = 0f;
                    }
                }
                else
                {
                    position += new Vector2(0f, moveDelta.y);
                }
            }

            // そのあとに横の移動を解決する。横の移動解決中に衝突した場合は、その地点で移動を終了する。
            if (!Mathf.Approximately(moveDelta.x, 0f))
            {
                var direction = new Vector2(Mathf.Sign(moveDelta.x), 0f);
                var distance = Mathf.Abs(moveDelta.x) + _config.Skin;

                var hit = Physics2D.BoxCast(position + _config.ColliderOffset, _config.ColliderSize, 0f, direction, distance, layerMask);

                if (hit.collider != null && hit.distance > 0f)
                {
                    var moveDistance = Mathf.Max(0f, hit.distance - _config.Skin);
                    position += direction * moveDistance;
                    _context.Velocity.x = 0f;
                }
                else
                {
                    position += new Vector2(moveDelta.x, 0f);
                }
            }

            _context.Position = position;
        }
    }
}