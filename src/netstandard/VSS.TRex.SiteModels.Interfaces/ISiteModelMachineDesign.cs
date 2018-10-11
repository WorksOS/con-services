using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMachineDesign: IBinaryReaderWriter
  {
    int Id { get; set; }

    string Name { get; set; }
  }
}
