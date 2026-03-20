using System.Collections.Generic;
using UnityEngine;

namespace Confront.Player.Movement
{
    public class DebugParams
    {
        public bool IsStepMode;
        public Vector2 Delta;
        public List<DebugPointInfo> DebugPoints = new List<DebugPointInfo>();
    }

    public struct DebugPointInfo
    {
        public DebugPointInfo(Vector3 point, Color color, float size = 0.01f)
        {
            Point = point;
            Color = color;
            Size = size;
        }

        public Vector3 Point;
        public Color Color;
        public float Size;
    }
}