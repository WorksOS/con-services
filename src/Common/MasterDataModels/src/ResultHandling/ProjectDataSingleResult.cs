using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Single project Descriptor result class
  /// </summary>
  public class ProjectDataSingleResult : BaseDataResult, IMasterDataModel
  {
    public ProjectData ProjectDescriptor { get;set; }

    public List<string> GetIdentifiers() => ProjectDescriptor?.GetIdentifiers() ?? new List<string>();
  }
}