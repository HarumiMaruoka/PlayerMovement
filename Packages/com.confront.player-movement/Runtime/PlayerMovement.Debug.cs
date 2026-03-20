#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Confront.Player.Movement
{
    public partial class PlayerMovement
    {
        public DebugParams DebugParams { get; } = new DebugParams();

        public void OnGUI()
        {
            if (_config == null && _context == null)
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

            void Space(float value = line)
            {
                y += value;
            }

            // 状態
            Row("Grounded", Bool(_context.IsGrounded) + $"   Angle: {OptF(_context.GroundAngle)}");
            Row("GroundNormal", OptV2(_context.GroundNormal));

            Space(4f);

            // 位置/速度
            Row("Position", V2(_context.Position));
            Row("Velocity", V2(_context.Velocity));

            Space(4f);

            // タイマー 
            Row("CoyoteTimer", F(_context.CoyoteTimer));
            Row("JumpHoldTimer", F(_context.JumpHoldTimer));
            Row("IgnoreOneWayTimer", F(_context.IgnoreOneWayPlatformTimer));
            Row("Delta", V2(DebugParams.Delta));

            Space();

            Row("StepMode", Bool(DebugParams.IsStepMode));
        }

        public void OnDrawGizmos(Transform player, MovementConfig config)
        {
            if (player == null || config == null)
            {
                return;
            }

            // Ground Prove
            var currentPosition = (Vector2)player.position;
            var boxRayOrigin = currentPosition + config.ColliderOffset + config.GroundProbeOffset;
            var boxRaySize = config.GroundProveSize;
            var rayAngle = 0f;
            var rayLength = config.GroundRayLength;
            var rayDirection = Vector2.down;
            var layerMask = config.GroundLayerMask | config.OneWayPlatformLayerMask;

            var groundHit = Physics2D.BoxCast(boxRayOrigin, boxRaySize, rayAngle, rayDirection, rayLength, layerMask);
            var isHit = groundHit.collider != null && groundHit.distance > 0;

            var endDistance = isHit ? groundHit.distance : rayLength;
            var endBoxOrigin = boxRayOrigin + rayDirection * endDistance;
            var startTop = boxRayOrigin + new Vector2(0f, boxRaySize.y * 0.5f);
            var endBottom = endBoxOrigin - new Vector2(0f, boxRaySize.y * 0.5f);

            Gizmos.color = isHit ? Color.red : Color.blue;
            Gizmos.DrawWireCube(boxRayOrigin, boxRaySize);
            Gizmos.DrawWireCube(endBoxOrigin, boxRaySize);
            Gizmos.DrawLine(startTop, endBottom);

            // Player Collider
            var colliderPosition = currentPosition + config.ColliderOffset;
            var colliderSize = config.ColliderSize;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(colliderPosition, colliderSize);

            // Debug Points
            if (DebugParams.DebugPoints != null && DebugParams.DebugPoints.Count > 0)
            {
                foreach (var pointInfo in DebugParams.DebugPoints)
                {
                    Gizmos.color = pointInfo.Color;
                    Gizmos.DrawWireSphere(pointInfo.Point, pointInfo.Size);
                }
            }
        }

        private void AddBoxPoints(Vector2 center, Vector2 boxSize, Color color, float circleSize = 0.01f)
        {
            var half = boxSize * 0.5f;
            DebugParams.DebugPoints.Add(new DebugPointInfo() { Point = center + new Vector2(-half.x, -half.y), Color = color, Size = circleSize });
            DebugParams.DebugPoints.Add(new DebugPointInfo() { Point = center + new Vector2(-half.x, half.y), Color = color, Size = circleSize });
            DebugParams.DebugPoints.Add(new DebugPointInfo() { Point = center + new Vector2(half.x, half.y), Color = color, Size = circleSize });
            DebugParams.DebugPoints.Add(new DebugPointInfo() { Point = center + new Vector2(half.x, -half.y), Color = color, Size = circleSize });
        }
    }
}
#endif