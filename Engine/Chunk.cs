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
    
    public Chunk(uint seed, Vector3 position)
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
        edgesMaterial = Raylib.LoadMaterialDefault();

        string path = "../../../Resources/Textures/";
    
        // 1. Load the Textures
        Texture2D colorTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_Color.jpg");
        Texture2D normalTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_NormalGL.jpg");
        Texture2D aoTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_AmbientOcclusion.jpg");
        Texture2D roughTex = Raylib.LoadTexture(path + "Rock058_4K-JPG_Roughness.jpg");

        // 2. Assign to Material Maps
        // Albedo is the base color
        Raylib.SetMaterialTexture(ref material, (int)MaterialMapIndex.Albedo, colorTex);
    
        // Normal map for surface detail
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Normal, normalTex);
    
        // Ambient Occlusion for baked shadows in crevices
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Occlusion, aoTex);
    
        // Roughness (Often assigned to the Roughness or Metalness index depending on the shader)
        Raylib.SetMaterialTexture(ref material, MaterialMapIndex.Roughness, roughTex);

        unsafe {
            material.Maps[(int)MaterialMapIndex.Albedo].Color = Color.White;
            edgesMaterial.Maps[(int)MaterialMapIndex.Albedo].Color = Color.Red;
        }

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
        mesh.TriangleCount = vertices.Count / 3;
        mesh.VertexCount = vertices.Count;

        mesh.AllocVertices();
        mesh.AllocTexCoords();
        mesh.AllocNormals();

        unsafe
        {
            CollectionsMarshal.AsSpan(vertices).CopyTo(new Span<Vector3>(mesh.Vertices, vertices.Count));
            CollectionsMarshal.AsSpan(uvs).CopyTo(new Span<Vector2>(mesh.TexCoords, uvs.Count));
            CollectionsMarshal.AsSpan(normals).CopyTo(new Span<Vector3>(mesh.Normals, normals.Count));
            CollectionsMarshal.AsSpan(colors).CopyTo(new Span<Color>(mesh.Colors, colors.Count));
        }
        
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