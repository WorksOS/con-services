using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Single project Descriptor result class
  /// </summary>
  public class ProjectDataSingleResult : BaseDataResult
  {
    public ProjectData ProjectDescriptor { get;set; }
  }
}