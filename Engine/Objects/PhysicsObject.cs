using System.Numerics;
using Engine.Abstractions;

namespace Engine.Objects;

public abstract class PhysicsObject(int mass, Vector3 positionVector) : IGameObject
{
    public float   Mass             = mass;
    public Vector3 PositionVector   = positionVector;
    public Vector3 Velocity         = Vector3.Zero;
    public Vector3 Acceleration     = Vector3.Zero;
    
    public void ApplyForce(Vector3 force)
    {
        this.Acceleration += force / Mass;
    }
    
    public void Update(float dt)
    {
        Velocity += Acceleration * dt;
        
        PositionVector += Velocity * dt;
        
        Acceleration = Vector3.Zero;
    }
}