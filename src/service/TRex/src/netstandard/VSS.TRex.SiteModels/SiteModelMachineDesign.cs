using System.IO;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Describes a single machine design used in the site model. 
  /// </summary>
  public class SiteModelMachineDesign : ISiteModelMachineDesign
  {
    private const int READER_WRITER_VERSION_MACHINE_DESIGN = 3;

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
      writer.Write(READER_WRITER_VERSION_MACHINE_DESIGN); 

      writer.Write(Id);
      writer.Write(Name);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Deserialises the machine design names using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != READER_WRITER_VERSION_MACHINE_DESIGN)
        throw new TRexSerializationVersionException(READER_WRITER_VERSION_MACHINE_DESIGN, version);

      Id = reader.ReadInt32();
      Name = reader.ReadString();
    }
  }
}
