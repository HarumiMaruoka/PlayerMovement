#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Game.Player.Movement
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

            void Space()
            {
                y += line;
            }

            // 状態
            Row("Grounded", Bool(_context.IsGrounded) + $"   Angle: {OptF(_context.GroundAngle)}");
            Row("GroundNormal", OptV2(_context.GroundNormal));

            y += 4f;

            // 位置/速度
            Row("Position", V2(_context.Position));
            Row("Velocity", V2(_context.Velocity));

            y += 4f;

            // タイマー 
            Row("CoyoteTimer", F(_context.CoyoteTimer));
            Row("JumpHoldTimer", F(_context.JumpHoldTimer));
            Row("IgnoreOneWayTimer", F(_context.IgnoreOneWayPlatformTimer));

            Space();

            Row("StepMode", Bool(DebugParams.IsStepMode));
        }

        public void OnDrawGizmos(Transform player, MovementConfig config)
        {

        }
    }
}
#endif