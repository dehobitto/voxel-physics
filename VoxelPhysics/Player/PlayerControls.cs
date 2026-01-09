using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace VoxelPhysics.Player;

public interface IPlayerControls
{
    public void Update(float dt, Player player);
}
public class WindowsPlayerControls : IPlayerControls
{
    public void Update(float dt, Player player)
    {
        if (IsKeyDown(KeyboardKey.Space))
        {
            Jump();
        }

        if (IsKeyDown(KeyboardKey.W))
        {
            var force = player.Camera.GetNormalizedFlatDirection();
            player.ApplyForce(force);
        }
        if (IsKeyDown(KeyboardKey.S))
        {
            var force = player.Camera.GetNormalizedFlatDirection();
            player.ApplyForce(-force);
        }
        if (IsKeyDown(KeyboardKey.A))
        {
            var force = Vector3.Normalize(Vector3.Cross(player.Camera.GetNormalizedFlatDirection(), Vector3.UnitY));
            player.ApplyForce(-force);
        }
        if (IsKeyDown(KeyboardKey.D))
        {
            var force = Vector3.Normalize(Vector3.Cross(player.Camera.GetNormalizedFlatDirection(), Vector3.UnitY));
            player.ApplyForce(force);
        }
    }
    
    private void Jump()
    {
        if (player.GetPlayerState() == State.Standing)
        {
            player.SetPlayerState(State.InAir);
            var force = Vector3.UnitY * player.Mass * (float)Math.Sqrt(2*9.8) * 5;
            player.ApplyForce(force);
        }
    }

    private void MoveForward()
    {
        throw new NotImplementedException();
    }

    private void MoveRight()
    {
        throw new NotImplementedException();
    }

    private void MoveLeft()
    {
        throw new NotImplementedException();
    }

    private void MoveBackwards()
    {
        throw new NotImplementedException();
    }
}