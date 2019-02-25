using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <inheritdoc />
  public class RaptorFileUploadUtility : IRaptorFileUploadUtility
  {
    private const long MAX_UPLOAD_SIZE_LIMIT = 20971520; //1024 * 1024 * 20

    private readonly ILogger log;

    /// <summary>
    /// Creates a file upload service to push files to the Raptor AS Node.
    /// </summary>
    public RaptorFileUploadUtility(ILoggerFactory logger)
    {
      log = logger.CreateLogger<RaptorFileUploadUtility>();
    }

    /// <inheritdoc />
    public (bool success, string message) UploadFile(FileDescriptor fileDescriptor, byte[] fileData)
    {
      using (var ms = new MemoryStream(fileData))
      {
        if (ms.Length > MAX_UPLOAD_SIZE_LIMIT)
        {
          return (success: false, message: $"File too large {ms.Length / 1024}kb; maximum allowed filesize is {MAX_UPLOAD_SIZE_LIMIT / 1024}kb.");
        }

        (bool folderCreated, string mappedFilenameResult) = BuildMappedFileName(fileDescriptor);

        if (!folderCreated) { return (false, mappedFilenameResult); }

        log.LogDebug($"Uploading file '{fileDescriptor.FileName}' ({ms.Length} bytes), to '{mappedFilenameResult}'");

        try
        {
          using (var file = File.OpenWrite(mappedFilenameResult))
          {
            if (ms.CanSeek) { ms.Seek(0, SeekOrigin.Begin); }

            ms.CopyTo(file);
          }
        }
        catch (Exception ex)
        {
          log.LogError($"Error uploading file '{fileDescriptor.FileName}' to '{fileDescriptor.Path}': {ex.GetBaseException().Message}");
        }

        log.LogDebug($"Successfully uploaded file '{mappedFilenameResult}'");
        return (success: true, null);
      }
    }

    /// <inheritdoc />
    public void DeleteFile(string filename)
    {
      if (string.IsNullOrEmpty(filename) || !File.Exists(filename)) { return; }

      log.LogDebug($"Deleting file '{filename}'");

      Task.Run(() => File.Delete(filename)).ContinueWith(t =>
      {
        if (t.Exception != null)
        {
          log.LogError($"Failed to delete temporary linework file '{filename}', error: {t.Exception.GetBaseException().Message}");
        }
      });
    }

    /// <summary>
    /// Creates the filename mapped to the temporary upload location on the Raptor AS Node host.
    /// </summary>
    private (bool success, string mappedFilenameResult) BuildMappedFileName(FileDescriptor fileDescriptor)
    {
      var dirInfo = new DirectoryInfo(fileDescriptor.Path);

      if (!dirInfo.Exists)
      {
        try
        {
          dirInfo.Create();
        }
        catch (Exception ex)
        {
          var errorMessage = $"Failed to create temporary upload folder '{dirInfo.FullName}', error: {ex.GetBaseException().Message}";
          log.LogError(errorMessage);

          return (false, errorMessage);
        }
      }

      return (true, Path.Combine(fileDescriptor.Path, fileDescriptor.FileName));
    }
  }
}
