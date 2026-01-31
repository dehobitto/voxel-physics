using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace Engine;

public class Chunk
{
    const int CHUNK_SIZE = 16;
    Block[,,] blocks;
    
    private Mesh mesh; 
    private Material material;
    private Matrix4x4 transform;
    
    private List<Vector3> vertices = new();
    
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
                    if (y == 0) blocks[x, y, z].IsActive = true; 
                }
            }
        }
        
        transform = Matrix4x4.CreateTranslation(position);
        material = Raylib.LoadMaterialDefault();
        
        GenerateMesh();
    }
    
    public void GenerateMesh()
    {
        vertices.Clear();

        bool faceByDefault = true;

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (!blocks[x, y, z].IsActive) continue;
                    
                    //TODO check for neighbours, and rename faceByDefault
                    
                    AddCubeToData(x, y, z);
                }
            }
        }
        
        BuildRaylibMesh();
    }
    
    void AddCubeToData(
        int x, int y, int z,
        bool xNeg = true, bool xPos = true,
        bool yNeg = true, bool yPos = true,
        bool zNeg = true, bool zPos = true)
    {
        // Top Face (+Y)
        if (!yPos)
        {
            vertices.Add(new Vector3(x,     y + 1, z));
            vertices.Add(new Vector3(x,     y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x,     y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
        }

        // Bottom Face (-Y)
        if (!yNeg)
        {
            vertices.Add(new Vector3(x,     y, z + 1));
            vertices.Add(new Vector3(x,     y, z));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x,     y, z));
            vertices.Add(new Vector3(x + 1, y, z));
        }

        // Front Face (+Z)
        if (!zPos)
        {
            vertices.Add(new Vector3(x,     y,     z + 1));
            vertices.Add(new Vector3(x + 1, y,     z + 1));
            vertices.Add(new Vector3(x,     y + 1, z + 1));
            vertices.Add(new Vector3(x,     y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y,     z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
        }

        // Back Face (-Z)
        if (!zNeg)
        {
            vertices.Add(new Vector3(x + 1, y,     z));
            vertices.Add(new Vector3(x,     y,     z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x,     y,     z));
            vertices.Add(new Vector3(x,     y + 1, z));
        }

        // Right Face (+X)
        if (!xPos)
        {
            vertices.Add(new Vector3(x + 1, y,     z + 1));
            vertices.Add(new Vector3(x + 1, y,     z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y,     z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
        }

        // Left Face (-X)
        if (!xNeg)
        {
            vertices.Add(new Vector3(x,     y,     z));
            vertices.Add(new Vector3(x,     y,     z + 1));
            vertices.Add(new Vector3(x,     y + 1, z));
            vertices.Add(new Vector3(x,     y + 1, z));
            vertices.Add(new Vector3(x,     y,     z + 1));
            vertices.Add(new Vector3(x,     y + 1, z + 1));
        }
    }

    void BuildRaylibMesh()
    {
        mesh = new Mesh();
        mesh.TriangleCount = vertices.Count / 3;
        mesh.VertexCount = vertices.Count;

        mesh.AllocVertices();

        unsafe
        {
            Span<Vector3> meshVertices = new Span<Vector3>(mesh.Vertices, vertices.Count);
            CollectionsMarshal.AsSpan(vertices).CopyTo(meshVertices);
        }
        
        Raylib.UploadMesh(ref mesh, false);
    }
    
    public void Draw()
    {
        Raylib.DrawMesh(mesh, material, transform);
    }
    
    public void Unload()
    {
        Raylib.UnloadMesh(mesh);
    }
}