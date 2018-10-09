namespace VSS.VersionTool
{
  public class Reference
  {
    public string Name { get; }
    public VersionNumber Version { get; }

    public Reference(string name, string version)
    {
      Name = name;
      Version = new VersionNumber(version);
    }

    public override string ToString()
    {
      return $"{Name} ({Version})";
    }
  }
}