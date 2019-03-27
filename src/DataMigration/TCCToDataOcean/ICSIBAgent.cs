using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean
{
  public interface ICSIBAgent
    { 
      Task<ContractExecutionResult> GetCSIBForProject(Project project);
      Task<JObject> GetCoordSysInfoFromCSIB64(Project project, string coordSysId);
      Task<string> GetCalibrationFileForCoordSysId(Project project, string csib);
  }
}
