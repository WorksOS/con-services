using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.Interfaces
{
  public interface ICSIBAgent
    { 
      Task<CSIBResult> GetCSIBForProject(Project project);
      Task<JObject> GetCoordSysInfoFromCSIB64(Project project, string coordSysId);
      Task<string> GetCalibrationFileForCoordSysId(Project project, string csib);
  }
}
