using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <inheritdoc />
  public class RaptorFileUploadUtility : IRaptorFileUploadUtility
  {
    private const long MAX_UPLOAD_SIZE_LIMIT = 20971520; //1024 * 1024 * 20

    private ILogger log;
    private IConfigurationStore configStore;

    private string GetUploadFolderRoot() => @"Z:\Temp\LineworkFileUploads\";

    /// <summary>
    /// Creates a file upload service to push files to the Raptor AS Node.
    /// </summary>
    public RaptorFileUploadUtility(ILoggerFactory logger, IConfigurationStore configurationStore)
    {
      log = logger.CreateLogger<RaptorFileUploadUtility>();
      configStore = configurationStore;
    }

    /// <inheritdoc />
    public (bool success, string message) UploadFile(FileDescriptor fileDescriptor, string fileDescriptorPathIdentifier, IFormFile fileData)
    {
      using (var ms = new MemoryStream())
      {
        fileData.CopyTo(ms);

        if (ms.Length > MAX_UPLOAD_SIZE_LIMIT)
        {
          return (success: false, message: $"File too large {ms.Length / 1024}kb; maximum allowed filesize is {MAX_UPLOAD_SIZE_LIMIT / 1024}kb.");
        }

        using (var file = File.OpenWrite(MappedFileName(fileDescriptor, fileDescriptorPathIdentifier)))
        {
          if (ms.CanSeek) ms.Seek(0, SeekOrigin.Begin);

          ms.CopyTo(file);
        }

        return (success: true, null);
      }
    }

    /// <summary>
    /// Creates the filename mapped to the temporary upload location on the Raptor AS Node host.
    /// </summary>
    public string MappedFileName(FileDescriptor fileDescriptor, string fileDescriptorPathIdentifier)
    {
      var mappedPath = Path.Combine(GetUploadFolderRoot(), fileDescriptorPathIdentifier);
      var dirInfo = new DirectoryInfo(mappedPath);

      if (!dirInfo.Exists)
      {
        try
        {
          dirInfo.Create();
        }
        catch (Exception ex)
        {
          log.LogWarning("Failed to create temporary upload folder {0}: {1}", mappedPath, ex.Message);
          throw ex;
        }
      }

      return Path.Combine(mappedPath, fileDescriptor.FileName);
    }
  }
}
