using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsDesignClient
  {
    Task<CreateFileResponseModel> CreateFile(Guid projectUid, CreateFileRequestModel createFileRequest, IHeaderDictionary customHeaders = null);
    Task<CreateFileResponseModel> CreateAndUploadFile(Guid projectUid, CreateFileRequestModel createFileRequest, Stream fileContents, IHeaderDictionary customHeaders = null);
  }
}
