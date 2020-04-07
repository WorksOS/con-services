using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using VSS.Productivity3D.FileAccess.WebAPI.Models.ResultHandling;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.FileAccess.WebAPI.Models.Executors
{
  public class RawFileAccessExecutor : RequestExecutorContainer
  {
    public RawFileAccessExecutor()
    { }

    public RawFileAccessExecutor(ILoggerFactory logger, IConfigurationStore configStore, IFileRepository fileAccess)
      : base(logger, configStore, fileAccess)
    { }

    /// <summary>
    /// Processes the raw file access request by getting the file from TCC and returning its contents as bytes.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      byte[] data = null;

      var request = item as FileDescriptor;
      log.LogInformation($"{nameof(RawFileAccessExecutor)}: FilespaceId: {request.FilespaceId}, Path: {request.Path}, Filename: {request.FileName}");

      try
      {
        if (fileAccess != null)
        {
          var stream = DownloadFile(request);

          if (stream?.Length > 0)
          {
            stream.Position = 0;
            data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);

            log.LogInformation($"{nameof(RawFileAccessExecutor)}: Succeeded in reading {request.FilespaceId}: {request.Path}/{request.FileName}");
          }
          else
          {
            log.LogInformation($"{nameof(RawFileAccessExecutor)}: Failed to read {request.FilespaceId}: {request.Path}/{request.FileName} (stream is 0 length)");
          }
        }
        else
        {
          log.LogInformation("Unable to log into TCC as RawFileAccessExecutor user.");
        }
      }
      catch (Exception ex)
      {
        log.LogError(ex, $"Failed on getting {request.FileName} file from TCC!");
      }

      return RawFileAccessResult.Create(data);
    }

    private Stream DownloadFile(FileDescriptor file)
    {
      var fullName = string.IsNullOrEmpty(file.FileName)
        ? file.Path.Replace(Path.DirectorySeparatorChar, '/')
        : Path.Combine(file.Path, file.FileName);

      log.LogDebug($"Fetching file: {file.FilespaceId}{fullName}");
      var downloadFileResult = fileAccess.GetFile(file.FilespaceId, fullName).Result;
      log.LogDebug($"{nameof(DownloadFile)}: File result length {downloadFileResult?.Length ?? 0} bytes");

      return downloadFileResult;
    }
  }
}
