using System;
using System.IO;
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
  public static class S3FileTransfer
  {

    const string S3DirectorySeparator = "/";

    private static readonly ILogger Log = Logging.Logger.CreateLogger("S3FileTransfer");

    /// <summary>
    /// Reads a file from S3 and places it locally
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <param name="targetPath"></param>
    public static async Task<bool> ReadFile(Guid siteModelUid, string fileName, string targetPath)
    {
      var s3Path = $"{siteModelUid}{S3DirectorySeparator}{fileName}";
      FileStreamResult fileStreamResult;

      try
      {
        fileStreamResult = await DIContext.Obtain<ITransferProxy>().Download(s3Path).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception reading design from s3:");
        return false;
      }

      if (string.IsNullOrEmpty(fileStreamResult.ContentType))
      {
        Log.LogInformation("Exception setting up download from S3.ContentType unknown, i.e. file doesn't exist.");
        return false;
      }

      try
      {
        var targetFullPath = Path.Combine(targetPath, fileName);
        using (fileStreamResult.FileStream)
        {
          using (var targetFileStream = File.Create(targetFullPath, (int)fileStreamResult.FileStream.Length))
          {
            fileStreamResult.FileStream.CopyTo(targetFileStream);
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception writing design file locally:");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Writes a file to S3
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    /// <param name="localFullPath"></param>
    /// <param name="s3FullPath"></param>
    public static bool WriteFile(string localFullPath, string s3FullPath)
    {
      try
      {
        using (var fileStream = File.Open(localFullPath, FileMode.Open, FileAccess.Read))
        {
          DIContext.Obtain<ITransferProxy>().Upload(fileStream, s3FullPath);
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception writing file to s3:");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Writes a file to S3 to a specific bucket
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    /// <param name="localFullPath"></param>
    /// <param name="s3FullPath"></param>
    /// <param name="awsBucketName"></param>
    public static bool WriteFileToBucket(string localFullPath, string s3FullPath, string awsBucketName)
    {
      try
      {
        using (var fileStream = File.Open(localFullPath, FileMode.Open, FileAccess.Read))
        {
          DIContext.Obtain<ITransferProxy>().UploadToBucket(fileStream, s3FullPath, awsBucketName);
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception writing file to s3. bucket: {awsBucketName} :");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Writes a file to S3
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    public static bool WriteFile(string sourcePath, Guid siteModelUid, string fileName, string destinationFileName = null)
    {
      var localFullPath = Path.Combine(sourcePath, fileName);

      if (destinationFileName != null)
        fileName = destinationFileName;

      var s3FullPath = $"{siteModelUid}{S3DirectorySeparator}{fileName}";
      return WriteFile(localFullPath, s3FullPath);
    }

    /// <summary>
    ///  Remove file from storage
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool RemoveFileFromBucket(Guid siteModelUid, string fileName)
    {
      bool res;
      try
      {
        var s3FullPath = $"{siteModelUid}{S3DirectorySeparator}{fileName}";
        res = DIContext.Obtain<ITransferProxy>().RemoveFromBucket(s3FullPath);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception removing file from storage. file: {fileName} :");
        return false;
      }
      return res;
    }

  }
}
