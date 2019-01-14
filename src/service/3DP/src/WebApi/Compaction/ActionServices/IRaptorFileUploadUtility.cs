using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// Common file handling utility class for transferring and working with files on the Raptor host.
  /// </summary>
  public interface IRaptorFileUploadUtility
  {
    /// <summary>
    /// Uploads a file to the Raptor host.
    /// </summary>
    (bool success, string message) UploadFile(FileDescriptor fileDescriptor, IFormFile fileData);

    /// <summary>
    /// Returns a unique hash id for use with creating unique monikers shorter than a standard Guid.
    /// </summary>
    string GenerateUniqueId();

    /// <summary>
    /// Asynchrously and safely deletes a file.
    /// </summary>
    /// <remarks>
    /// For performance/network latency reasons, this method deletes the file asynchronously.
    /// </remarks>
    void DeleteFile(string filename);
  }
}
