using System.Numerics;
using System.Runtime.Versioning;
using Engine;
using Raylib_cs;

namespace VoxelPhysics;

using static Raylib;

internal static class Program
{
    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [STAThread]
    public static void Main()
    {
        var general = new General();
        
        InitWindow(general.WindowWidth, general.WindowHeight, general.WindowTitle);
        
        var player = new Player.Player(mass: 1, positionVector: -Vector3.One * 20);
        
        SetTargetFPS(144);
        DisableCursor();
        
        var chunkManager = new ChunkManager();
        
        var chunk = new Chunk(1, Vector3.Zero);
        chunkManager._chunks.Add(Vector3.Zero, chunk);

        while (!WindowShouldClose())
        {
            float dt = GetFrameTime();
            
            player.Update(dt);
            
            BeginDrawing();
                ClearBackground(Color.SkyBlue);
                DrawText($"X: {player.Camera.GetDirection().X}\nY: {player.GetDirection().Y}\nZ:{player.GetDirection().Z}", 10, 10, 10, Color.Black);

                BeginMode3D(player.GetCamera());
                    DrawPlane(Vector3.Zero, new Vector2(50f), Color.DarkGray);
                    //DrawCube(Vector3.UnitY, Vector3.One * 0.5f, Color.Red);
                    DrawGrid(10, 1.0f);
                    chunkManager.Draw();
                    
                EndMode3D();

            EndDrawing();
        }

        CloseWindow();
    }
}