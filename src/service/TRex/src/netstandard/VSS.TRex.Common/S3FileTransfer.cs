using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.Common
{
  /// <summary>
  /// Provides an interface to transferProxy for read or write.
  /// </summary>
  public class S3FileTransfer : IS3FileTransfer
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<S3FileTransfer>();

    const string S3DirectorySeparator = "/";

    private readonly TransferProxyType _type;

    /// <summary>
    /// Represents the underlying AWS S3 transfer proxy if require 
    /// </summary>
    public ITransferProxy Proxy { get; }

    public S3FileTransfer(TransferProxyType type)
    {
      _type = type;
      Proxy = DIContext.ObtainRequired<ITransferProxyFactory>().NewProxy(_type);
    }
    
    /// <summary>
    /// Reads a file from S3 and places it locally
    /// </summary>
    public async Task<bool> ReadFile(Guid siteModelUid, string fileName, string targetPath)
    {
      var s3Path = GetS3FullPath(siteModelUid, fileName);
      FileStreamResult fileStreamResult;

      try
      {
        fileStreamResult = await Proxy.Download(s3Path).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception reading design from s3:");
        return false;
      }

      if (string.IsNullOrEmpty(fileStreamResult.ContentType))
      {
        _log.LogInformation("Exception setting up download from S3.ContentType unknown, i.e. file doesn't exist.");
        return false;
      }

      try
      {
        var targetFullPath = Path.Combine(targetPath, fileName);
        await using (fileStreamResult.FileStream)
        {
          await using (var targetFileStream = File.Create(targetFullPath, (int)fileStreamResult.FileStream.Length))
          {
            fileStreamResult.FileStream.CopyTo(targetFileStream);
          }
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception writing design file locally:");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Reads a file from S3 and places it locally
    /// </summary>
    public bool ReadFileSync(Guid siteModelUid, string fileName, string targetPath)
    {
      var s3Path = GetS3FullPath(siteModelUid, fileName);
      FileStreamResult fileStreamResult;

      try
      {
        fileStreamResult = Proxy.DownloadSync(s3Path);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception reading design from s3:");
        return false;
      }

      if (string.IsNullOrEmpty(fileStreamResult.ContentType))
      {
        _log.LogInformation("Exception setting up download from S3.ContentType unknown, i.e. file doesn't exist.");
        return false;
      }

      try
      {
        var targetFullPath = Path.Combine(targetPath, fileName);
        using var stream = fileStreamResult.FileStream;
        using var targetFileStream = File.Create(targetFullPath, (int)fileStreamResult.FileStream.Length);
        fileStreamResult.FileStream.CopyTo(targetFileStream);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception writing design file locally:");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Writes a file to S3
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    public bool WriteFile(string localFullPath, string s3FullPath)
    {
      try
      {
        using var fileStream = File.Open(localFullPath, FileMode.Open, FileAccess.Read);
        Proxy.Upload(fileStream, s3FullPath);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception writing file to s3:");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Writes a file to S3 to a specific bucket
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    public bool WriteFileToBucket(string localFullPath, string s3FullPath, string awsBucketName)
    {
      try
      {
        using var fileStream = File.Open(localFullPath, FileMode.Open, FileAccess.Read);
        Proxy.UploadToBucket(fileStream, s3FullPath, awsBucketName);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception writing file to s3. bucket: {awsBucketName} :");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Writes a file to S3
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    public bool WriteFile(string sourcePath, Guid siteModelUid, string fileName, string destinationFileName = null)
    {
      var localFullPath = Path.Combine(sourcePath, fileName);

      if (destinationFileName != null)
        fileName = destinationFileName;

      var s3FullPath = GetS3FullPath(siteModelUid, fileName);
      return WriteFile(localFullPath, s3FullPath);
    }

    /// <summary>
    /// Writes a file to S3 and returns boolean & out url path to S3 location
    /// </summary>
    public bool WriteFile(string localFullPath, Guid siteModelUid, out string preSignedUrl)
    {
      preSignedUrl = string.Empty;
      var fileName = $"{Path.GetFileNameWithoutExtension(localFullPath)}-{Guid.NewGuid()}{Path.GetExtension(localFullPath)}";
      var s3FullPath = GetS3FullPath(siteModelUid, fileName);
      var ret = WriteFile(localFullPath, s3FullPath);
      if (ret)
        preSignedUrl = GeneratePreSignedUrl(s3FullPath);
      return ret;
    }

    /// <summary>
    ///  Remove file from storage
    /// </summary>
    public bool RemoveFileFromBucket(Guid siteModelUid, string fileName)
    {
      bool res;
      try
      {
        var s3FullPath = GetS3FullPath(siteModelUid, fileName);
        res = Proxy.RemoveFromBucket(s3FullPath);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception removing file from storage. file: {fileName} :");
        return false;
      }
      return res;
    }

    /// <summary>
    /// Generate url path to file located on S3
    /// </summary>
    public string GeneratePreSignedUrl(string path)
    {
      return Proxy.GeneratePreSignedUrl(path);
    }

    /// <summary>
    /// Returns a (possible incomplete) collection from the bucket that match the given prefix.
    /// The continuationToken is non-null/non-empty is there are more keys to query
    /// </summary>
    public Task<(string[], string)> ListKeys(string prefix, int maxKeys, string continuationToken)
    {
      try
      {
        return Proxy.ListKeys(prefix, maxKeys, continuationToken);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception listing keys fpr prefix {prefix}");
        return Task.FromResult((new string[0], ""));
      }
    }

    private string GetS3FullPath(Guid siteModelUid, string fileName) => $"{siteModelUid}{S3DirectorySeparator}{fileName}";

  }
}
