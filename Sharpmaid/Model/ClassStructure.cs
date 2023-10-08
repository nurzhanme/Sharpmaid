namespace Sharpmaid.Model;

public class ClassStructure
{
    public string FullName { get; set; }

    public string Name { get; set; }

    public string? ParentName { get; set; }
    public List<string> Methods { get; set; }
    public List<(string text, string typename)> Properties { get; set; }
}