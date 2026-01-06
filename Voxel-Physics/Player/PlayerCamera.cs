using System.Numerics;
using Raylib_cs;

namespace Voxel;

public class PlayerCamera
{
    internal Camera3D RaylibCamera;
    
    private float _phi   = 0.0f;
    private float _theta = MathF.PI / 2.0f;
    
    
    private Vector3 _direction;

    public PlayerCamera(Vector3 position)
    {
        RaylibCamera = new Camera3D
        {
            Projection = CameraProjection.Perspective,
            Up = Vector3.UnitY,
            FovY = 45f,
            Position = position,
            Target = Vector3.Zero
        };
        
        
    }

    public Vector3 GetDirection() => _direction;
    
    public Vector3 GetNormalizedFlatDirection()
    {
        Vector3 normalizedVector = Vector3.Normalize(_direction);
        normalizedVector.Y = 0;
        return normalizedVector;
    }

    public void Update(Vector3 pos)
    {
        Vector2 mouseDelta = Raylib.GetMouseDelta();
        float sensitivity = 0.003f;

        _phi -= mouseDelta.X * sensitivity;
        _theta += mouseDelta.Y * sensitivity;
        
        _theta = Math.Clamp(_theta, 0.01f, MathF.PI - 0.01f);
        
        // https://en.wikipedia.org/wiki/Spherical_coordinate_system
        _direction = new Vector3(
            MathF.Sin(_theta) * MathF.Sin(_phi),
            MathF.Cos(_theta),                  
            MathF.Sin(_theta) * MathF.Cos(_phi) 
        );
        
        RaylibCamera.Position = pos + Vector3.UnitY * 1.8f;
        RaylibCamera.Target = RaylibCamera.Position + _direction;
        Console.WriteLine($"CAMERA UPDATES {mouseDelta}");
    }
}