using System.Numerics;
using Raylib_cs;
using Engine;

namespace VoxelPhysics;

using static Raylib;

internal static unsafe class Program
{
    [STAThread]
    public static void Main()
    {
        var general = new General();
        
        InitWindow(general.WindowWidth, general.WindowHeight, general.WindowTitle);
        
        var player = new Player.Player(mass: 1, positionVector: -Vector3.One * 20);
        
        SetTargetFPS(144);
        DisableCursor();
        SetConfigFlags(ConfigFlags.Msaa4xHint); 
        
        var chunkManager = new ChunkManager();
        
        // Загрузка
        var shader = LoadShader("../../../Resources/Shaders/pbr.vs", "../../../Resources/Shaders/pbr.fs");

        // === ПРИВЯЗКА УНИФОРМ (Обязательно) ===
        // Мы говорим шейдеру: "Переменная 'albedoMap' берет данные из слота текстуры 0", и так далее.
        // Raylib при отрисовке меша сам кладет текстуры в слоты 0,1,2... согласно MaterialMapIndex.

        // Слот 0: Albedo
        int locAlbedo = GetShaderLocation(shader, "albedoMap");
        // Слот 1: Metallic (Raylib: Metalness)
        int locMetal = GetShaderLocation(shader, "metalMap");
        // Слот 2: Normal (Raylib: Normal)
        int locNormal = GetShaderLocation(shader, "normalMap");
        // Слот 3: Roughness (Raylib: Roughness)
        int locRough = GetShaderLocation(shader, "roughMap");
        // Слот 4: AO (Raylib: Occlusion)
        int locAO = GetShaderLocation(shader, "aoMap");
        // Слот 6: Displacement (Raylib: Height)
        int locDisp = GetShaderLocation(shader, "displacementMap");

        // Устанавливаем значения uniform-сэмплеров (индексы текстурных юнитов)
        // Это нужно сделать ОДИН раз после загрузки шейдера
        int texUnit0 = 0; SetShaderValue(shader, locAlbedo, &texUnit0, ShaderUniformDataType.Int);
        int texUnit1 = 1; SetShaderValue(shader, locMetal, &texUnit1, ShaderUniformDataType.Int);
        int texUnit2 = 2; SetShaderValue(shader, locNormal, &texUnit2, ShaderUniformDataType.Int);
        int texUnit3 = 3; SetShaderValue(shader, locRough, &texUnit3, ShaderUniformDataType.Int);
        int texUnit4 = 4; SetShaderValue(shader, locAO, &texUnit4, ShaderUniformDataType.Int);
        int texUnit6 = 6; SetShaderValue(shader, locDisp, &texUnit6, ShaderUniformDataType.Int);

        // Настройка других параметров
        shader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(shader, "viewPos");

        // Ambient (фоновый свет, чтобы не было черноты)
        Vector3 ambientColor = new Vector3(0.5f, 0.5f, 0.5f);
        SetShaderValue(shader, GetShaderLocation(shader, "ambientColor"), &ambientColor, ShaderUniformDataType.Vec3);
        float ambientInt = 0.5f;
        SetShaderValue(shader, GetShaderLocation(shader, "ambientIntensity"), &ambientInt, ShaderUniformDataType.Float);

        // Сила дисплейсмента (важно!)
        float dispScale = 0.1f; // Начни с малого значения
        SetShaderValue(shader, GetShaderLocation(shader, "displacementScale"), &dispScale, ShaderUniformDataType.Float);

// ... дальше цикл ламп и отрисовки ...
// ==========================

// ... далее создание lights ...
        
        var lights = new PbrLight[4];
        lights[0] = PbrLights.CreateLight(
            0,
            PbrLightType.Point,
            new Vector3(-1.0f, 1.0f, -2.0f),
            new Vector3(0.0f, 0.0f, 0.0f),
            Color.RayWhite,
            80.0f,
            shader);
        lights[1] = PbrLights.CreateLight(1,
            PbrLightType.Point,
            new Vector3(2.0f, 1.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 0.0f),
            Color.Green,
            3.3f,
            shader);
        lights[2] = PbrLights.CreateLight(
            2, PbrLightType.Point,
            new Vector3(-2.0f, 1.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 0.0f),
            Color.Red,
            8.3f,
            shader);
        lights[3] = PbrLights.CreateLight(
            3,
            PbrLightType.Point,
            new Vector3(1.0f, 1.0f, -2.0f),
            new Vector3(0.0f, 0.0f, 0.0f),
            Color.Black,
            2.0f,
            shader);
        
        var chunk = new Chunk(1, Vector3.Zero, shader);
        chunkManager._chunks.Add(Vector3.Zero, chunk);
        
        var usage = 1;
        SetShaderValue(shader, GetShaderLocation(shader, "useTexAlbedo"), &usage, ShaderUniformDataType.Int);
        SetShaderValue(shader, GetShaderLocation(shader, "useTexNormal"), &usage, ShaderUniformDataType.Int);
        SetShaderValue(shader, GetShaderLocation(shader, "useTexMRA"), &usage, ShaderUniformDataType.Int);
        SetShaderValue(shader, GetShaderLocation(shader, "useTexEmissive"), &usage, ShaderUniformDataType.Int);

        while (!WindowShouldClose())
        {
            float dt = GetFrameTime();
            
            player.Update(dt);
            
            Camera3D camera = player.GetCamera();
            
            var cameraPos = camera.Position;
            SetShaderValue(shader, shader.Locs[(int)ShaderLocationIndex.VectorView], cameraPos, ShaderUniformDataType.Vec3);
            
            for (var i = 0; i < 4; i++)
            {
                UpdateLight(shader, lights[i]);
            }
            
            BeginDrawing();
                ClearBackground(Color.SkyBlue);
                BeginMode3D(camera);

                        DrawPlane(Vector3.Zero, new Vector2(50f), Color.DarkGray);
                        DrawGrid(10, 1.0f);
                        chunk.Draw();
                        
                        for (var i = 0; i < 4; i++)
                        {
                            var color = lights[i].Color;
                            var lightColor = new Color((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255),
                                (byte)(color.W * 255));

                            if (lights[i].Enabled)
                            {
                                DrawSphereEx(lights[i].Position, 0.2f, 8, 8, lightColor);
                            }
                            else
                            {
                                DrawSphereWires(lights[i].Position, 0.2f, 8, 8, ColorAlpha(lightColor, 0.3f));
                            }
                        }

                EndMode3D();
                
                DrawFPS(10, 10);

            EndDrawing();
        }
        


        CloseWindow();
    }
    private static void UpdateLight(Shader shader, PbrLight light)
    {
        SetShaderValue(shader, light.EnabledLoc, &light.Enabled, ShaderUniformDataType.Int);
        SetShaderValue(shader, light.TypeLoc, &light.Type, ShaderUniformDataType.Int);
        SetShaderValue(shader, light.PositionLoc, &light.Position, ShaderUniformDataType.Vec3);
        SetShaderValue(shader, light.TargetLoc, &light.Target, ShaderUniformDataType.Vec3);
        SetShaderValue(shader, light.ColorLoc, &light.Color, ShaderUniformDataType.Vec4);
        SetShaderValue(shader, light.IntensityLoc, &light.Intensity, ShaderUniformDataType.Float);
    }
}