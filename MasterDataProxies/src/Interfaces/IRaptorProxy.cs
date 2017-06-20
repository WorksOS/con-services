using System;
using MasterDataProxies.ResultHandling;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MasterDataProxies.Interfaces
{
  public interface IRaptorProxy
  {
    Task<CoordinateSystemSettings> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFilename,
        IDictionary<string, string> customHeaders = null);

    Task<CoordinateSystemSettings> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFilename,
            IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> AddFile(Guid projectUid, Guid fileUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> DeleteFile(Guid projectUid, Guid fileUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids, IDictionary<string, string> customHeaders = null);

  }
}

