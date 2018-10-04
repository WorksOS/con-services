using System;
using System.IO;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Describes a single machine design used in the site model. 
  /// </summary>
  public class SiteModelMachineDesign : ISiteModelMachineDesign
  {
    public int Id { get; set; }
    public string Name { get; set; }

    public SiteModelMachineDesign()
    {
    }

    public SiteModelMachineDesign(int id, string name) 
    {
      Id = id;
      Name = name;
    }
    
    /// <summary>
    /// Serialises machine design names using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write((int)1); //Version number

      writer.Write(Id);
      writer.Write(Name);
    }

    /// <summary>
    /// Deserialises the machine design names using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != 1)
        throw new Exception($"Invalid version number ({version}) reading machine design names, expected version (1)");

      Id = reader.ReadInt16();
      Name = reader.ReadString();
    }
  }
}
