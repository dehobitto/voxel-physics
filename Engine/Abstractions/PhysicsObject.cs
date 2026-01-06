using System.Numerics;

namespace Engine.Abstractions;

public abstract class PhysicsObject(int mass, Vector3 positionVector)
{
    public int     Mass     = mass;
    public Vector3 PositionVector = positionVector;
    public Vector3 Velocity = Vector3.Zero;
    
    public virtual void ApplyForce(Vector3 force)
    {
        this.Velocity += force / Mass;
    }
    
    protected virtual void UpdatePhysics(float deltatime)
    {
        PositionVector += Velocity * deltatime;
    }
}