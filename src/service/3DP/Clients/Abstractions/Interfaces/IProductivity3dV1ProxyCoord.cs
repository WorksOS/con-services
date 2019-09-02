using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV1ProxyCoord : IProductivity3dV1Proxy
  {
    Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent,
      string coordinateSystemFilename,
      IDictionary<string, string> customHeaders = null);

    Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent,
      string coordinateSystemFilename,
      IDictionary<string, string> customHeaders = null);
  }
}
