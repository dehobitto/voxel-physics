using System.Numerics;
using Engine.Abstractions;
using Raylib_cs;

namespace VoxelPhysics.Player;

public class Player : PhysicsObject
{
    internal PlayerCamera Camera;
    private State _state;
    
    public State GetPlayerState() => _state;
    public void SetPlayerState(State state) => _state = state;

    public Player(int mass, Vector3 positionVector) : base(mass, positionVector)
    {
        Camera = new PlayerCamera(positionVector);

        _state = State.Standing;
    }

    private void HandlePhysics(float dt)
    {
        if (_state == State.InAir)
        {
            ApplyForce(Vector3.UnitY * -9.8f);
        }

        this.UpdatePhysics(dt);
        
        if (PositionVector.Y <= 0)
        {
            _state = State.Standing;
            PositionVector.Y = 0;
            if (Velocity.Y < 0) Velocity.Y = 0;
        }
    }

    public void Update(float dt)
    {
        Camera.Update(PositionVector);
        HandlePhysics(dt);
    }
    
    public Camera3D GetCamera()    => Camera.RaylibCamera;
    public Vector3  GetDirection() => Camera.GetDirection();
}