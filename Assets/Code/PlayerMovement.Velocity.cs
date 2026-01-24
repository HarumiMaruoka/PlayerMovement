using UnityEngine;

namespace Game.Player.Movement
{
    public partial class PlayerMovement
    {
        // 目標速度計算
        private void UpdateVelocity(float deltaTime, MovementInput movementInput)
        {
            // 水平方向の速度計算
            DetermineMovementParameters(movementInput, out float targetSpeed, out float acceleration, out float deceleration, out float turnSpeed);
            var inputDir = movementInput.Move.x != 0f ? Mathf.Sign(movementInput.Move.x) : 0f; // 水平方向の入力方向: -1, 0, 1
            var currentDir = _context.Velocity.x != 0f ? Mathf.Sign(_context.Velocity.x) : 0f; // 現在の水平方向の移動方向: -1, 0, 1

            if (inputDir != 0f)
            {
                // 入力がある場合
                if (inputDir == currentDir || currentDir == 0f)
                {
                    // 同じ方向への入力、または停止状態からの入力
                    _context.Velocity.x = Mathf.MoveTowards(_context.Velocity.x, inputDir * targetSpeed, acceleration * deltaTime);
                }
                else
                {
                    // 逆方向への入力
                    _context.Velocity.x = Mathf.MoveTowards(_context.Velocity.x, 0f, turnSpeed * deltaTime);
                }
            }
            else
            {
                // 入力がない場合
                _context.Velocity.x = Mathf.MoveTowards(_context.Velocity.x, 0f, deceleration * deltaTime);
            }

            // 垂直方向の速度計算
            if (!_context.IsGrounded)
            {
                if (_context.CoyoteTimer > 0f && movementInput.Jump)
                {
                    // コヨーテタイマーが有効でジャンプ入力がある場合、垂直速度をジャンプ速度に設定
                    _context.Velocity.y = _config.JumpForce;
                    _context.CoyoteTimer = 0f; // コヨーテタイマーを消費
                    _context.JumpHoldTimer = _config.JumpHoldTimeRemaining; // 可変ジャンプタイマーをリセット
                }
                // 重力加算
                else if (_context.JumpHoldTimer > 0f && movementInput.Jump)
                {
                    // ジャンプバッファタイマーが有効でジャンプ入力がある場合、重力を軽減
                    _context.Velocity.y -= _config.Gravity * _config.JumpBufferGravityMultiplier * deltaTime;
                }
                else
                {
                    // 通常の重力適用
                    _context.Velocity.y -= _config.Gravity * deltaTime;
                }

                // 最大落下速度制限
                if (_context.Velocity.y < _config.AirMaxFallSpeed)
                {
                    _context.Velocity.y = _config.AirMaxFallSpeed;
                }
            }
            else
            {
                // 地面に接地している場合、垂直速度を0にリセット
                _context.Velocity.y = 0f;

                if (movementInput.Jump)
                {
                    // ジャンプ入力がある場合、垂直速度をジャンプ速度に設定
                    _context.Velocity.y = _config.JumpForce;
                    _context.JumpHoldTimer = _config.JumpHoldTimeRemaining; // 可変ジャンプタイマーをリセット
                }
            }
        }

        private void DetermineMovementParameters(MovementInput movementInput, out float targetSpeed, out float acceleration, out float deceleration, out float turnSpeed)
        {
            if (_context.IsGrounded)
            {
                if (movementInput.Sprint)
                {
                    targetSpeed = _config.SprintMaxSpeed;
                    acceleration = _config.SprintAcceleration;
                    deceleration = _config.SprintDeceleration;
                    turnSpeed = _config.SprintTurnSpeed;
                }
                else
                {
                    targetSpeed = _config.WalkMaxSpeed;
                    acceleration = _config.WalkAcceleration;
                    deceleration = _config.WalkDeceleration;
                    turnSpeed = _config.WalkTurnSpeed;
                }
            }
            else
            {
                targetSpeed = _config.AirHorizontalMaxSpeed;
                acceleration = _config.AirControlAcceleration;
                deceleration = _config.AirControlDeceleration;
                turnSpeed = _config.AirTurnSpeed;
            }
        }
    }
}