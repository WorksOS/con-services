using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.Common
{
  /// <summary>
  /// Provides an interface to transferProxy for read or write.
  /// </summary>
  public class S3FileTransfer 
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Reads a file from S3 and places it locally
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <param name="targetPath"></param>
    public static async Task<bool> ReadFile(Guid siteModelUid, string fileName, string targetPath)
    {
      var s3Path = $"{siteModelUid.ToString()}/{fileName}";
      FileStreamResult fileStreamResult;

      try
      {
        fileStreamResult = await DIContext.Obtain<ITransferProxy>().Download(s3Path).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        // todojeannie what to do about retries?
        Log.LogError($"Exception reading design from s3: {e}");
        return false;
      }

      if (string.IsNullOrEmpty(fileStreamResult.ContentType))
      {
        Log.LogInformation($"Exception setting up download from S3.ContentType unknown, i.e. file doesn't exist.");
        return false;
      }

      try
      {
        var targetFullPath = Path.Combine(targetPath, fileName);
        using (var targetFileStream = System.IO.File.Create(targetFullPath, (int)fileStreamResult.FileStream.Length))
        {
          fileStreamResult.FileStream.CopyTo(targetFileStream);
        }
      }
      catch (Exception e)
      {
        Log.LogError($"Exception writing design file locally: {e}");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Writes a file to S3
    ///  AWS' ransferUtility will create the 'directory' if not already there
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    public static bool WriteFile(string sourcePath, Guid siteModelUid, string fileName )
    {
      var localFullPath = Path.Combine(sourcePath, fileName);
      var s3FullPath = $"{siteModelUid.ToString()}/{fileName}";
      try
      {
        var fileStream = System.IO.File.Open(localFullPath, FileMode.Open, FileAccess.Read);
        DIContext.Obtain<ITransferProxy>().Upload(fileStream, s3FullPath);
      }
      catch (Exception e)
      {
        Log.LogError($"Exception writing design to s3: {e}");
        return false;
      }
      return true;
    }
  }
}
