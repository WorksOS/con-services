using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsDesignClient
  {
    Task<CreateFileResponseModel> CreateFile(Guid projectUid, CreateFileRequestModel createFileRequest, IDictionary<string, string> customHeaders = null);
    Task<CreateFileResponseModel> CreateAndUploadFile(Guid projectUid, CreateFileRequestModel createFileRequest, Stream fileContents, IDictionary<string, string> customHeaders = null);
  }
}
