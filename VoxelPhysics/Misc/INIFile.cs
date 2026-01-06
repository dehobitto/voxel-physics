using System.Text;

namespace VoxelPhysics.Misc;

public class IniFile
{
    private string Path { get; set; }

    public IniFile(string iniPath)
    {
        Path = iniPath;
        // Create the file if it doesn't exist to prevent errors
        if (!File.Exists(Path)) File.WriteAllText(Path, "", Encoding.UTF8);
    }
    
    public void IniWriteValue(string section, string key, string value)
    {
        var lines = File.ReadAllLines(Path).ToList();
        string sectionHeader = $"[{section}]";
        int sectionIndex = lines.FindIndex(l => l.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase));

        if (sectionIndex == -1)
        {
            // Section doesn't exist, add it to the end
            lines.Add(sectionHeader);
            lines.Add($"{key}={value}");
        }
        else
        {
            // Look for the key within the section
            bool foundKey = false;
            for (int i = sectionIndex + 1; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith("[")) break; // Started a new section
                if (lines[i].Contains("=") && lines[i].Split('=')[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"{key}={value}";
                    foundKey = true;
                    break;
                }
            }
            if (!foundKey) lines.Insert(sectionIndex + 1, $"{key}={value}");
        }

        File.WriteAllLines(Path, lines);
    }
        
    public string IniReadValue(string section, string key)
    {
        if (!File.Exists(Path)) return string.Empty;

        var lines = File.ReadAllLines(Path);
        string sectionHeader = $"[{section}]";
        bool inSection = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Equals(sectionHeader, StringComparison.OrdinalIgnoreCase))
            {
                inSection = true;
                continue;
            }

            if (inSection)
            {
                if (trimmed.StartsWith("[")) break; // New section starts
                if (trimmed.Contains("="))
                {
                    var parts = trimmed.Split('=', 2);
                    if (parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return parts[1].Trim();
                    }
                }
            }
        }
        return string.Empty;
    }

    public int IniReadInt(string section, string key)
    {
        string value = IniReadValue(section, key);
        return int.TryParse(value, out int result) ? result : 0;
    }
}