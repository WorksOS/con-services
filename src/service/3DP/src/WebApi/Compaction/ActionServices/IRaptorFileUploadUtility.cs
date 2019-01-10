using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// 
  /// </summary>
  public interface IRaptorFileUploadUtility
  {
    /// <summary>
    /// Uploads a file to the Raptor host.
    /// </summary>
    (bool success, string message) UploadFile(FileDescriptor fileDescriptor, string fileDescriptorPathIdentifier, IFormFile fileData);
  }
}
