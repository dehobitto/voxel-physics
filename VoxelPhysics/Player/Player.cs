using System.Numerics;
using Engine.Abstractions;
using Engine.Objects;
using Raylib_cs;

namespace VoxelPhysics.Player;

public class Player : PhysicsObject
{
    internal PCamera Camera;
    private State _state;
    
    public State GetPlayerState() => _state;
    public void SetPlayerState(State state) => _state = state;
    private PMovementSystem pms;

    public Player(int mass, Vector3 positionVector) : base(mass, positionVector)
    {
        Camera = new PCamera(positionVector);
        pms = new WindowsPMS(player: this);
        _state = State.Standing;
    }

    private void HandlePhysics(float dt)
    {
        if (_state == State.InAir)
        {
            ApplyForce(Vector3.UnitY * -9.8f * Mass);
        }
        
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
        pms.Update(dt);
        HandlePhysics(dt);
        
        base.Update(dt);
    }
    
    public Camera3D GetCamera()    => Camera.RaylibCamera;
    public Vector3  GetDirection() => Camera.GetDirection();
}