using System;
using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        private MovementConfig _config;
        private MovementContext _context;

        // Initialization
        public void Start(MovementConfig config, MovementContext context = null)
        {
            if (!config)
            {
                throw new System.ArgumentNullException(nameof(config), "MovementConfig cannot be null");
            }

            _config = config;

            _context = context ?? new MovementContext();
        }

        public void Update(float deltaTime, Transform player, MovementInput movementInput)
        {
            _context.Position = player.position;

            UpdateTimer(deltaTime);
            // Prove Grounded
            GroundProve();
            // Calculate Velocity
            UpdateVelocity(deltaTime, movementInput);
            // Apply Movement
            ApplyMovement(deltaTime);

            player.position = _context.Position;
        }

        private void UpdateTimer(float deltaTime)
        {
            // Update various timers in the context
            if (_context == null)
            {
                return;
            }

            if (_context.CoyoteTimer > 0f)
            {
                _context.CoyoteTimer -= deltaTime;
            }
            if (_context.JumpHoldTimer > 0f)
            {
                _context.JumpHoldTimer -= deltaTime;
            }
            if (_context.IgnoreOneWayPlatformTimer > 0f)
            {
                _context.IgnoreOneWayPlatformTimer -= deltaTime;
            }
        }
    }
}