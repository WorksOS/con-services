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

    Task<ContractExecutionResult> AddFile(long? projectId, Guid? projectUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> DeleteFile(long? projectId, Guid? projectUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

  }
}

