using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeTypes;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;

namespace VSS.AWS.TransferProxy
{
  public class LocalTransferProxy : ITransferProxy
  {
    /// <summary>
    /// The location in the local temp folder to create buckets representing S3 buckets local S3 clients will access in place of AWS::S3
    /// </summary>
    private readonly string _rootLocalTransferProxyFolder = Path.Combine(Path.GetTempPath(), "MockLocalS3Store");

    private readonly string _awsBucketName;
    private readonly ILogger _logger;

    public LocalTransferProxy(IConfigurationStore configStore, ILogger<LocalTransferProxy> log, string storageKey)
    {
      _logger = log;
      if (string.IsNullOrEmpty(storageKey))
      {
        throw new ArgumentException($"Missing environment variable {storageKey}", nameof(storageKey));
      }
      _awsBucketName = configStore.GetValueString(storageKey);

      _logger.LogInformation($"AWS S3 using local storage in {_rootLocalTransferProxyFolder} with default bucket folder {_awsBucketName}");
    }

    public async Task<FileStreamResult> DownloadFromBucket(string s3Key, string bucketName)
    {
      var localKey = (s3Key.StartsWith("/") ? s3Key.Substring(1) : s3Key).Replace('/', Path.DirectorySeparatorChar);
      var fileName = Path.Combine(_rootLocalTransferProxyFolder, bucketName, localKey);

      return new FileStreamResult(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), ContentTypeConstants.ApplicationOctetStream);
    }

    /// <summary>
    /// Create a task to download a file from the local S3 storage
    /// </summary>
    /// <param name="s3Key">Key to the data to be downloaded</param>
    /// <returns>FileStreamResult if the file exists</returns>
    public async Task<FileStreamResult> Download(string s3Key) => await DownloadFromBucket(s3Key, _awsBucketName);

    public string GeneratePreSignedUrl(string s3Key)
    {
      return $"http://dummyLocalPreSignedURL/{s3Key}";
    }

    public void Upload(Stream stream, string s3Key) => UploadToBucket(stream, s3Key, _awsBucketName);

    /// <summary>
    /// Upload a file to the local S3 storage. The filepath will contain the extension matched to the content type if not already present
    /// </summary>
    /// <param name="stream">Content stream to upload</param>
    /// <param name="s3Key">s3 key (filename) to content</param>
    /// <param name="contentType">Content Type of the data being uploaded (defines extension of the filename)</param>
    /// <returns>Path to the file (could be renamed based on the contentType)</returns>
    /// <exception cref="ArgumentException">If invalid contentType is passed in</exception>
    public string Upload(Stream stream, string s3Key, string contentType)
    {
      var extension = Path.GetExtension(s3Key);
      var expectedExtension = MimeTypeMap.GetExtension(contentType);

      var path = s3Key;

      if (string.Compare(expectedExtension, extension, StringComparison.InvariantCultureIgnoreCase) != 0)
      {
        // Do we want to replace the existing extension???
        path = s3Key + expectedExtension;
      }

      Upload(stream, path);
      return path;
    }

    public void UploadToBucket(Stream stream, string s3Key, string bucketName)
    {
      var localKey = (s3Key.StartsWith("/") ? s3Key.Substring(1) : s3Key).Replace('/', Path.DirectorySeparatorChar);
      var fileName = Path.Combine(_rootLocalTransferProxyFolder, bucketName, localKey);
      var directory = Path.GetDirectoryName(fileName);

      if (!Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }

      using (var fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
      {
        stream.CopyTo(fs);
      }
    }
  }
}
