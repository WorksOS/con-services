using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.MasterDataProxies.ResultHandling;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IRaptorProxy
  {
    Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFilename,
        IDictionary<string, string> customHeaders = null);

    Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFilename,
            IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> AddFile(Guid projectUid, Guid fileUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> DeleteFile(Guid projectUid, Guid fileUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> ProjectSettingsValidate(string settings, IDictionary<string, string> customHeaders = null);
  }
}

