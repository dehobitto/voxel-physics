using System.Numerics;

namespace Engine;

public class ChunkManager
{
    const int CHUNK_SIZE = 16;
    const int RENDER_DISTANCE = 2;
    
    public Dictionary<Vector3, Chunk> _chunks = new();

    public void Update(float dt, Vector3 cameraPosition)
    {
        int chunkX = (int)Math.Floor(cameraPosition.X / CHUNK_SIZE);
        int chunkY = (int)Math.Floor(cameraPosition.Y / CHUNK_SIZE); 
        int chunkZ = (int)Math.Floor(cameraPosition.Z / CHUNK_SIZE);
        
        UpdateLoadList(new Vector3(chunkX, 0, chunkZ));
    }

    void UpdateLoadList(Vector3 centerChunkPos)
    {
        for (int x = -RENDER_DISTANCE; x <= RENDER_DISTANCE; x++)
        {
            for (int z = -RENDER_DISTANCE; z <= RENDER_DISTANCE; z++)
            {
                Vector3 targetPos = centerChunkPos + new Vector3(x, 0, z);
                
                if (!_chunks.ContainsKey(targetPos))
                {
                    Vector3 worldPos = targetPos * CHUNK_SIZE;
                    
                    Chunk newChunk = new Chunk(12345, worldPos);
                    _chunks.Add(targetPos, newChunk);
                }
            }
        }
    }
    
    public void Draw()
    {
        foreach (var chunk in _chunks.Values)
        {
            chunk.Draw();
        }
    }
}