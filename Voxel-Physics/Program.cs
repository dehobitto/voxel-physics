using System.Numerics;
using Raylib_cs;

namespace Voxel;

using static Raylib;

internal static class Program
{
    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [STAThread]
    public static void Main()
    {
        var general = new General();
        
        InitWindow(general.WindowWidth, general.WindowHeight, general.WindowTitle);
        
        var player = new Player(90, Vector3.Zero, Vector3.Zero);
        
        SetTargetFPS(60);
        DisableCursor();

        while (!WindowShouldClose())
        {
            float dt = GetFrameTime();

            if (IsKeyDown(KeyboardKey.Space))
            {
                if (player.GetPlayerState() == State.Standing)
                {
                    player.SetPlayerState(State.InAir);
                    var force = Vector3.UnitY * player.Mass * (float)Math.Sqrt(2*9.8) * 5;
                    player.ApplyForce(force);
                }
            }

            if (IsKeyDown(KeyboardKey.W))
            {
                var force = player.Camera.GetNormalizedFlatDirection();
                Console.WriteLine("SKJ");
                Console.WriteLine(GetMousePosition());
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
            
            player.Update(dt);
            
            BeginDrawing();
                ClearBackground(Color.Green);
                DrawText($"X: {player.Camera.GetDirection().X}\nY: {player.GetDirection().Y}\nZ:{player.GetDirection().Z}", 10, 10, 10, Color.Black);

                BeginMode3D(player.GetCamera());
                    DrawPlane(Vector3.Zero, new Vector2(50f), Color.DarkGray);
                    DrawCube(Vector3.UnitY, Vector3.One * 0.5f, Color.Red);
                    DrawGrid(10, 1.0f);
                EndMode3D();

            EndDrawing();
        }

        CloseWindow();
    }
    
    public static void DrawCube(Vector3 position, Vector3 size, Color color)
    {
        Raylib.DrawCube(position, size.X, size.Y, size.Z, color);
    }
}