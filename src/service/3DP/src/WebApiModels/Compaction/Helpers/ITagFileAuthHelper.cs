using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface ITagFileAuthHelper
  {
    Task<GetProjectUidsResult> GetProjectUid(string eCSerial, double machineLatitude, double machineLongitude);
  }
}
