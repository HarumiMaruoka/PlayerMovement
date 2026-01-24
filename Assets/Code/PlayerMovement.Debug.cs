using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        public void OnGUI()
        {
            // GUI表示：速度、接地状態など
        }

        public void OnDrawGizmos(Transform player)
        {
            // Gizmos表示：接地判定の可視化など
        }
    }
}