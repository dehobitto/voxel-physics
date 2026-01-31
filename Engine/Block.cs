namespace Engine;

public class Block
{
    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    private BlockType _blockType;
}

enum BlockType
{
    BlockType_Default = 0,
}