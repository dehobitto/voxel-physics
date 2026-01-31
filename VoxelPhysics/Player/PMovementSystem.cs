using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace VoxelPhysics.Player;

public abstract class PMovementSystem(Player player)
{
    public Player Player = player;
    public Dictionary<KeyboardKey, Action> ActionBindings;
    public abstract void Update(float dt);
}

public class WindowsPMS : PMovementSystem
{
    public WindowsPMS(Player player) : base(player)
    {
        ActionBindings = new Dictionary<KeyboardKey, Action>
        {
            {KeyboardKey.W, MoveForward},
            {KeyboardKey.A, MoveLeft},
            {KeyboardKey.S, MoveBackwards},
            {KeyboardKey.D, MoveRight},
            
            {KeyboardKey.Space, Jump}
        };
    }

    public override void Update(float dt)
    {
        foreach (var (keyboardKey, action) in ActionBindings)
        {
            if (IsKeyDown(keyboardKey))
            {
                action();
            }
        }
    }
    
    private void Jump()
    {
        if (Player.GetPlayerState() == State.Standing)
        {
            Player.SetPlayerState(State.InAir);
            var force = Vector3.UnitY * Player.Mass * (float)Math.Sqrt(2*9.8) * 80;
            Player.ApplyForce(force);
        }
    }
    private void MoveForward()
    {
        var force = Player.Camera.GetNormalizedFlatDirection();
        force *= Player.Mass;
        Player.ApplyForce(force);
    }
    private void MoveRight()
    {
        var force = Vector3.Normalize(Vector3.Cross(Player.Camera.GetNormalizedFlatDirection(), Vector3.UnitY));
        Player.ApplyForce(force);
    }
    private void MoveLeft()
    {
        var force = Vector3.Normalize(Vector3.Cross(Player.Camera.GetNormalizedFlatDirection(), Vector3.UnitY));
        Player.ApplyForce(-force);
    }
    private void MoveBackwards()
    {
        var force = Player.Camera.GetNormalizedFlatDirection();
        Player.ApplyForce(-force);
    }
}