using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace Engine;

using static Math;

public class Chunk
{
    const int CHUNK_SIZE = 16;
    Block[,,] blocks;
    
    private Mesh mesh; 
    private Material material;
    private Material edgesMaterial;
    private Matrix4x4 transform;
    
    private List<Vector3> vertices = new();
    private List<Vector2> uvs = new();
    private List<Vector3> normals = new();
    
    private List<Color> colors = new();
    
    public Chunk(uint seed, Vector3 position, Shader shader)
    {
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    blocks[x, y, z] = new Block(); 
                }
            }
        }
        
        var diameter = CHUNK_SIZE;
        for (int z = 0; z < diameter; z++) {
            for (int y = 0; y < diameter; y++) {
                for (int x = 0; x < diameter; x++) {
                    if (Sqrt((float)(x - diameter / 2) * (x - diameter / 2) + (y - diameter / 2) * (y - diameter / 2) + (z - diameter / 2) * (z - diameter / 2)) <= diameter / 2) {
                        blocks[x, y, z].IsActive = true;
                    }
                }
            }
        }
        
        transform = Matrix4x4.CreateTranslation(position);
        
        material = Raylib.LoadMaterialDefault();
        material.Shader = shader;
        edgesMaterial = Raylib.LoadMaterialDefault();

        string path = "../../../Resources/Textures/";
    
        // 1. Load the Textures
        Texture2D colorTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_Color.jpg");
        Texture2D normalTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_NormalGL.jpg");
        Texture2D aoTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_AmbientOcclusion.jpg");
        Texture2D roughTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_Roughness.jpg");
        Texture2D dispTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_Displacement.jpg");
        int dispLoc = Raylib.GetShaderLocation(material.Shader, "displacementMap");
        
        // В конструкторе Chunk, после загрузки textures:

// 1. Albedo -> Слот 0
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Albedo, colorTex);

// 2. Metallic -> Слот 1
// Если у тебя нет карты металла (для камней она черная),
// создай 1-пиксельную черную текстуру или загрузи любую заглушку.
// Иначе шейдер будет читать мусор.
        Image imgMetal = Raylib.GenImageColor(4, 4, Color.Black);
        Texture2D texMetal = Raylib.LoadTextureFromImage(imgMetal);
        Raylib.UnloadImage(imgMetal);
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Metalness, texMetal);

// 3. Normal -> Слот 2
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Normal, normalTex);

// 4. Roughness -> Слот 3
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Roughness, roughTex);

// 5. AO -> Слот 4
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Occlusion, aoTex);

// 6. Displacement -> Слот 6 (Height)
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Height, dispTex);

// ВАЖНО для Дисплейсмента:
// Чтобы вершины смещались, меш должен иметь достаточно полигонов. 
// Твой меш сейчас состоит из пары треугольников на грань. 
// Смещение будет двигать весь угол целиком, это может порвать геометрию.
// Но шейдер работать будет.

        GenerateMesh();
    }
    
    public void GenerateMesh()
    {
        vertices.Clear();
        uvs.Clear();
        normals.Clear();

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (!blocks[x, y, z].IsActive) continue;

                    bool xNeg = (x > 0) ? blocks[x - 1, y, z].IsActive : false;
                    bool xPos = (x < CHUNK_SIZE - 1) ? blocks[x + 1, y, z].IsActive : false;
                    bool yNeg = (y > 0) ? blocks[x, y - 1, z].IsActive : false;
                    bool yPos = (y < CHUNK_SIZE - 1) ? blocks[x, y + 1, z].IsActive : false;
                    bool zNeg = (z > 0) ? blocks[x, y, z - 1].IsActive : false;
                    bool zPos = (z < CHUNK_SIZE - 1) ? blocks[x, y, z + 1].IsActive : false;
                    
                    AddCubeToData(x, y, z, xNeg, xPos, yNeg, yPos, zNeg, zPos);
                }
            }
        }
        
        BuildRaylibMesh();
    }
    
    void AddCubeToData(int x, int y, int z, bool xNeg, bool xPos, bool yNeg, bool yPos, bool zNeg, bool zPos)
    {
        if (!yPos) AddFace(x, y, z, "top");
        if (!yNeg) AddFace(x, y, z, "bottom");
        if (!zPos) AddFace(x, y, z, "front");
        if (!zNeg) AddFace(x, y, z, "back");
        if (!xPos) AddFace(x, y, z, "right");
        if (!xNeg) AddFace(x, y, z, "left");
    }

    private void AddFace(int x, int y, int z, string side)
    {
        Vector3 n = side switch {
            "top" => Vector3.UnitY, "bottom" => -Vector3.UnitY,
            "front" => Vector3.UnitZ, "back" => -Vector3.UnitZ,
            "right" => Vector3.UnitX, _ => -Vector3.UnitX
        };

        for(int i = 0; i < 6; i++) normals.Add(n);

        if (side == "top") {
            vertices.Add(new(x, y+1, z)); uvs.Add(new(0, 0));
            vertices.Add(new(x, y+1, z+1)); uvs.Add(new(0, 1));
            vertices.Add(new(x+1, y+1, z)); uvs.Add(new(1, 0));
            vertices.Add(new(x+1, y+1, z)); uvs.Add(new(1, 0));
            vertices.Add(new(x, y+1, z+1)); uvs.Add(new(0, 1));
            vertices.Add(new(x+1, y+1, z+1)); uvs.Add(new(1, 1));
        }
        else if (side == "bottom") {
            vertices.Add(new(x, y, z+1)); uvs.Add(new(0, 0));
            vertices.Add(new(x, y, z)); uvs.Add(new(0, 1));
            vertices.Add(new(x+1, y, z+1)); uvs.Add(new(1, 0));
            vertices.Add(new(x+1, y, z+1)); uvs.Add(new(1, 0));
            vertices.Add(new(x, y, z)); uvs.Add(new(0, 1));
            vertices.Add(new(x+1, y, z)); uvs.Add(new(1, 1));
        }
        else if (side == "front") {
            vertices.Add(new(x, y, z+1)); uvs.Add(new(0, 1));
            vertices.Add(new(x+1, y, z+1)); uvs.Add(new(1, 1));
            vertices.Add(new(x, y+1, z+1)); uvs.Add(new(0, 0));
            vertices.Add(new(x, y+1, z+1)); uvs.Add(new(0, 0));
            vertices.Add(new(x+1, y, z+1)); uvs.Add(new(1, 1));
            vertices.Add(new(x+1, y+1, z+1)); uvs.Add(new(1, 0));
        }
        else if (side == "back") {
            vertices.Add(new(x+1, y, z)); uvs.Add(new(0, 1));
            vertices.Add(new(x, y, z)); uvs.Add(new(1, 1));
            vertices.Add(new(x+1, y+1, z)); uvs.Add(new(0, 0));
            vertices.Add(new(x+1, y+1, z)); uvs.Add(new(0, 0));
            vertices.Add(new(x, y, z)); uvs.Add(new(1, 1));
            vertices.Add(new(x, y+1, z)); uvs.Add(new(1, 0));
        }
        else if (side == "right") {
            vertices.Add(new(x+1, y, z+1)); uvs.Add(new(0, 1));
            vertices.Add(new(x+1, y, z)); uvs.Add(new(1, 1));
            vertices.Add(new(x+1, y+1, z+1)); uvs.Add(new(0, 0));
            vertices.Add(new(x+1, y+1, z+1)); uvs.Add(new(0, 0));
            vertices.Add(new(x+1, y, z)); uvs.Add(new(1, 1));
            vertices.Add(new(x+1, y+1, z)); uvs.Add(new(1, 0));
        }
        else if (side == "left") {
            vertices.Add(new(x, y, z)); uvs.Add(new(0, 1));
            vertices.Add(new(x, y, z+1)); uvs.Add(new(1, 1));
            vertices.Add(new(x, y+1, z)); uvs.Add(new(0, 0));
            vertices.Add(new(x, y+1, z)); uvs.Add(new(0, 0));
            vertices.Add(new(x, y, z+1)); uvs.Add(new(1, 1));
            vertices.Add(new(x, y+1, z+1)); uvs.Add(new(1, 0));
        }
    }

    void BuildRaylibMesh()
    {
        if (mesh.VertexCount > 0) Raylib.UnloadMesh(mesh);
        
        mesh = new Mesh();
        mesh.TriangleCount = vertices.Count / 3; // Внимание: если vertices хранит просто точки, а не треугольники, тут может быть ошибка. Но у вас вроде AddFace добавляет по 6 вершин, так что ок.
        mesh.VertexCount = vertices.Count;

        mesh.AllocVertices();
        mesh.AllocTexCoords();
        mesh.AllocNormals();
        // ВАЖНО: Выделяем память под тангенсы, иначе GenMeshTangents некуда писать
        mesh.AllocTangents(); 
        
        // Добавляем цвета, если используете
        // mesh.AllocColors(); 

        unsafe
        {
            CollectionsMarshal.AsSpan(vertices).CopyTo(new Span<Vector3>(mesh.Vertices, vertices.Count));
            CollectionsMarshal.AsSpan(uvs).CopyTo(new Span<Vector2>(mesh.TexCoords, uvs.Count));
            CollectionsMarshal.AsSpan(normals).CopyTo(new Span<Vector3>(mesh.Normals, normals.Count));
            
            // Если используете цвета вершин
            // CollectionsMarshal.AsSpan(colors).CopyTo(new Span<Color>(mesh.Colors, colors.Count));
        }
        
        // Теперь генерация тангенсов сработает корректно
        Raylib.GenMeshTangents(ref mesh);
        
        Raylib.UploadMesh(ref mesh, false);
    }
    
    public void Draw()
    {
        Raylib.DrawMesh(mesh, material, transform);
        Rlgl.EnableWireMode();
        //Raylib.DrawMesh(mesh, edgesMaterial, transform);
        Rlgl.DisableWireMode();
    }
    
    public void Unload()
    {
        Raylib.UnloadMesh(mesh);
        Raylib.UnloadMaterial(material);
        Raylib.UnloadMaterial(edgesMaterial);
    }
}