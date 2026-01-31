#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        public void OnGUI()
        {
            if (_config == null || !_config.ShowGizmos)
            {
                return;
            }

            if (_context == null)
            {
                return;
            }

            const float pad = 10f;
            const float width = 360f;
            const float line = 18f;

            var area = new Rect(pad, pad, width, 260f);
            GUI.color = new Color(1f, 1f, 1f, 0.95f);
            GUI.Box(area, "Movement Debug");

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true,
                wordWrap = false,
                alignment = TextAnchor.UpperLeft
            };

            float y = pad + 24f;
            float x = pad + 12f;

            string Bool(bool v) => v ? "<color=#7CFC00>ON</color>" : "<color=#FF6A6A>OFF</color>";
            string F(float v) => v.ToString("0.000");
            string V2(Vector2 v) => $"({v.x:0.000}, {v.y:0.000})";
            string OptF(float? v) => v.HasValue ? v.Value.ToString("0.00") : "-";
            string OptV2(Vector2? v) => v.HasValue ? V2(v.Value) : "-";

            void Row(string key, string value)
            {
                GUI.Label(new Rect(x, y, width - 24f, line), $"<b>{key}</b>  {value}", labelStyle);
                y += line;
            }

            // 状態
            Row("Grounded", Bool(_context.IsGrounded) + $"   Angle: {OptF(_context.GroundAngle)}");
            Row("GroundNormal", OptV2(_context.GroundNormal));

            y += 4f;

            // 位置/速度
            Row("Position", V2(_context.Position));
            Row("Velocity", V2(_context.Velocity));

            // 移動量
            Row("Delta", V2(_context.Delta));
            // 移動解決時の衝突情報
            Row("LastMoveHitNormal", OptV2(_context.LastMoveHitNormal));

            y += 4f;

            // タイマー
            Row("CoyoteTimer", F(_context.CoyoteTimer));
            Row("JumpHoldTimer", F(_context.JumpHoldTimer));
            Row("IgnoreOneWayTimer", F(_context.IgnoreOneWayPlatformTimer));
        }

        public void OnDrawGizmos(Transform player, MovementConfig config)
        {
            if (!player || !config)
            {
                return;
            }

            if (!config.ShowGizmos)
            {
                return;
            }

            // Gizmos表示：接地判定の可視化など

            // 接地判定のGizmo表示
            if (config.ShowGroundProbeGizmos)
            {
                var position = (Vector2)player.position + config.ColliderOffset;

                var hit = ProbeGroundCircleCast(position, config, out var groundProbeOrigin, out var groundProbeRadius,
                    out var direction, out var groundProbeLength, out int layerMask);
                Gizmos.color = Color.green;
                // 開始位置の円
                Gizmos.DrawWireSphere(groundProbeOrigin + direction * groundProbeLength, groundProbeRadius);
                // 終了位置の円
                Gizmos.DrawWireSphere(groundProbeOrigin, groundProbeRadius);
                // 左右の線
                var left = new Vector2(-groundProbeRadius, 0f);
                var right = new Vector2(groundProbeRadius, 0f);
                Gizmos.DrawLine(groundProbeOrigin + direction * groundProbeLength + left, groundProbeOrigin + left);
                Gizmos.DrawLine(groundProbeOrigin + direction * groundProbeLength + right, groundProbeOrigin + right);
            }

            // カプセルキャストのGizmo表示
            // ApplyMovement で使用している CapsuleCast（移動、ステップアップ、地面吸着）を可視化する
            if (!(config.ShowMovementCapsuleCastGizmos || config.ShowStepUpCapsuleCastGizmos || config.ShowGroundSnapCapsuleCastGizmos))
            {
                return;
            }

            var startPosition = (Vector2)player.position;

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                deltaTime = 1f / 60f;
            }

            var velocity = _context != null ? _context.Velocity : Vector2.zero;
            var delta = velocity * deltaTime;

            var castOrigin = startPosition + config.ColliderOffset;
            var castSize = config.ColliderSize;
            var capsuleDirection = CapsuleDirection2D.Vertical;
            var castAngle = 0f;
            var castDirection = delta.sqrMagnitude > 0f ? delta.normalized : Vector2.right;
            var castLength = delta.magnitude + config.Skin;

            // UnityのCapsuleCastは中心点を起点とするため、中心から概形を描く
            var verticalRadius = Mathf.Min(castSize.x, castSize.y) * 0.5f;
            var verticalSegment = Mathf.Max(0f, castSize.y - castSize.x);
            var hemiOffset = new Vector2(0f, verticalSegment * 0.5f);

            // 移動カプセルキャスト
            if (config.ShowMovementCapsuleCastGizmos)
            {
                var topCenter = castOrigin + hemiOffset;
                var bottomCenter = castOrigin - hemiOffset;

                // 現在位置のカプセル
                Gizmos.color = new Color(0f, 0.6f, 1f, 1f);
                Gizmos.DrawWireSphere(topCenter, verticalRadius);
                Gizmos.DrawWireSphere(bottomCenter, verticalRadius);
                var left = new Vector2(-verticalRadius, 0f);
                var right = new Vector2(verticalRadius, 0f);
                Gizmos.DrawLine(topCenter + right, bottomCenter + right);
                Gizmos.DrawLine(topCenter + left, bottomCenter + left);

                // // 移動カプセルキャストの終点位置
                // var endOrigin = castOrigin + castDirection * castLength;
                // var endTopCenter = endOrigin + hemiOffset;
                // var endBottomCenter = endOrigin - hemiOffset;
                // Gizmos.color = new Color(0f, 1f, 0.4f, 1f);
                // Gizmos.DrawWireSphere(endTopCenter, verticalRadius);
                // Gizmos.DrawWireSphere(endBottomCenter, verticalRadius);
                // Gizmos.DrawLine(endTopCenter + right, endBottomCenter + right);
                // Gizmos.DrawLine(endTopCenter + left, endBottomCenter + left);
                // Gizmos.DrawLine(castOrigin, endOrigin);
                //
                // // 実際のヒット位置
                // var castHit = Physics2D.CapsuleCast(castOrigin, castSize, capsuleDirection, castAngle, castDirection, castLength, config.GroundLayerMask);
                // if (castHit.collider != null)
                // {
                //     Gizmos.color = Color.red;
                //     Gizmos.DrawSphere(castHit.point, 0.05f);
                //     Gizmos.DrawLine(castOrigin, castHit.point);
                // }
            }

            // ステップアップ用カプセルキャスト
            if (config.ShowStepUpCapsuleCastGizmos && config.StepHeight > 0f)
            {
                var left = new Vector2(-verticalRadius, 0f);
                var right = new Vector2(verticalRadius, 0f);

                var stepUpOrigin = castOrigin + new Vector2(0f, config.StepHeight);
                var stepTopCenter = stepUpOrigin + hemiOffset;
                var stepBottomCenter = stepUpOrigin - hemiOffset;

                Gizmos.color = new Color(1f, 0.6f, 0f, 1f);
                Gizmos.DrawWireSphere(stepTopCenter, verticalRadius);
                Gizmos.DrawWireSphere(stepBottomCenter, verticalRadius);
                Gizmos.DrawLine(stepTopCenter + right, stepBottomCenter + right);
                Gizmos.DrawLine(stepTopCenter + left, stepBottomCenter + left);

                var stepEndOrigin = stepUpOrigin + castDirection * castLength;
                Gizmos.DrawLine(stepUpOrigin, stepEndOrigin);

                var stepHit = Physics2D.CapsuleCast(stepUpOrigin, castSize, capsuleDirection, castAngle, castDirection, castLength, config.GroundLayerMask);
                if (stepHit.collider != null)
                {
                    Gizmos.color = new Color(1f, 0.2f, 0f, 1f);
                    Gizmos.DrawSphere(stepHit.point, 0.05f);
                    Gizmos.DrawLine(stepUpOrigin, stepHit.point);
                }
            }

            // 地面吸着用カプセルキャスト（下方向）
            if (config.ShowGroundSnapCapsuleCastGizmos && config.StepHeight > 0f)
            {
                var left = new Vector2(-verticalRadius, 0f);
                var right = new Vector2(verticalRadius, 0f);

                var snapOrigin = (startPosition + delta) + config.ColliderOffset + new Vector2(0f, config.StepHeight);
                var snapLen = config.StepHeight + config.Skin;

                var snapTopCenter = snapOrigin + hemiOffset;
                var snapBottomCenter = snapOrigin - hemiOffset;

                Gizmos.color = new Color(0.6f, 0f, 1f, 1f);
                Gizmos.DrawWireSphere(snapTopCenter, verticalRadius);
                Gizmos.DrawWireSphere(snapBottomCenter, verticalRadius);
                Gizmos.DrawLine(snapTopCenter + right, snapBottomCenter + right);
                Gizmos.DrawLine(snapTopCenter + left, snapBottomCenter + left);

                var snapEndOrigin = snapOrigin + Vector2.down * snapLen;
                Gizmos.DrawLine(snapOrigin, snapEndOrigin);

                var snapHit = Physics2D.CapsuleCast(snapOrigin, castSize, capsuleDirection, castAngle, Vector2.down, snapLen, config.GroundLayerMask);
                if (snapHit.collider != null)
                {
                    Gizmos.color = new Color(0.9f, 0f, 1f, 1f);
                    Gizmos.DrawSphere(snapHit.point, 0.05f);
                    Gizmos.DrawLine(snapOrigin, snapHit.point);
                }
            }
        }
    }
}
#endif