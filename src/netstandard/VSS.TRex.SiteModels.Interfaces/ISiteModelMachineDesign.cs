using System.IO;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMachineDesign
  {
    int Id { get; set; }

    string Name { get; set; }

    void Write(BinaryWriter writer);
  
    void Read(BinaryReader reader);
  }
}
