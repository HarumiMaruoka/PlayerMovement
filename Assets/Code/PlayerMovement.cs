using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        private MovementConfig _config;
        private MovementContext _context;

        public void Start(MovementConfig config, MovementContext context = null)
        {
            // Initialization
            if (!config)
            {
                throw new System.ArgumentNullException(nameof(config), "MovementConfig cannot be null");
            }

            _config = config;

            _context = context ?? new MovementContext();
        }

        public void Update(Transform player, MovementInput movementInput)
        {
            _context.Position = player.position;

            // Prove Grounded
            Prove();
            // Calculate Velocity
            UpdateVelocity(movementInput);
            // Apply Movement
            ApplyMovement();

            player.position = _context.Position;
        }
    }
}