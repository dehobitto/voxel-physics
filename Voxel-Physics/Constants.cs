namespace Voxel;

using Misc;

public class General
{
    private const string PATH = """C:\CODE\C#\Voxel-Physics\mcsharp\UserSettings.ini""";
    
    public readonly int WindowWidth;
    public readonly int WindowHeight;
    public readonly string WindowTitle;

    public General()
    {
        var iniFile = new IniFile(PATH);
        this.WindowWidth = iniFile.IniReadInt("GENERAL", "WindowWidth");
        this.WindowHeight = iniFile.IniReadInt("GENERAL", "WindowHeight");
        this.WindowTitle = iniFile.IniReadValue("GENERAL", "WindowTitle");
    }
}

public enum State
{
    InAir,
    Standing
}