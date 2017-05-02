using MasterDataProxies.ResultHandling;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.Common.Interfaces
{
  public interface IRaptorProxy
  {
    Task<CoordinateSystemSettings> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFilename,
        IDictionary<string, string> customHeaders = null);

    Task<CoordinateSystemSettings> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFilename,
            IDictionary<string, string> customHeaders = null);
  }
}

