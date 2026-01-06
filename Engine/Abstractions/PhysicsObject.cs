using System.Numerics;

namespace Engine.Abstractions;

public abstract class PhysicsObject(int mass, Vector3 position, Vector3 velocity)
{
    public int     Mass     = mass;
    public Vector3 Position = position;
    public Vector3 Velocity = velocity;
    
    public virtual void ApplyForce(Vector3 force)
    {
        this.Velocity += force / Mass;
    }
    
    protected virtual void UpdatePhysics(float deltatime)
    {
        Position += Velocity * deltatime;
    }
}