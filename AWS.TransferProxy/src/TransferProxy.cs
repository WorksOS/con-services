using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using MimeTypes;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;

namespace VSS.AWS.TransferProxy
{
  public class TransferProxy : ITransferProxy
  {
    private readonly string awsAccessKey;
    private readonly string awsSecretKey;
    private readonly string awsBucketName;
    private readonly TimeSpan awsLinkExpiry;

    private const int MAXIMUM_EXPIRY_DAYS_FOR_PRESIGNED_URL = 7;

    public TransferProxy(IConfigurationStore configStore)
    {
      awsAccessKey = configStore.GetValueString("AWS_ACCESS_KEY");
      awsSecretKey = configStore.GetValueString("AWS_SECRET_KEY");
      awsBucketName = configStore.GetValueString("AWS_BUCKET_NAME");

      awsLinkExpiry = configStore.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY") ?? TimeSpan.FromDays(MAXIMUM_EXPIRY_DAYS_FOR_PRESIGNED_URL);

      if (string.IsNullOrEmpty(awsAccessKey) ||
          string.IsNullOrEmpty(awsSecretKey) ||
          string.IsNullOrEmpty(awsBucketName))
      {
        throw new Exception("Missing environment variable AWS_ACCESS_KEY, AWS_SECRET_KEY or AWS_BUCKET_NAME");
      }
    }

    /// <summary>
    /// Create a task to download a file from S3 storage
    /// </summary>
    /// <param name="s3Key">Key to the data to be downloaded</param>
    /// <returns>FileStreamResult if the file exists</returns>
    public async Task<FileStreamResult> Download(string s3Key)
    {
      using (var transferUtil = new TransferUtility(awsAccessKey, awsSecretKey, RegionEndpoint.USWest2))
      {
        var stream = await transferUtil.OpenStreamAsync(awsBucketName, s3Key);
        string mimeType;
        try
        {
          var extension = Path.GetExtension(s3Key);

          mimeType = MimeTypeMap.GetMimeType(extension);
        }
        catch (ArgumentException)
        {
          // Unknown or invalid extension, not bad but we don't know the content type
          mimeType = "application/octet-stream"; // binary data....
        }

        return new FileStreamResult(stream, mimeType);
      }
    }

    public void Upload(Stream stream, string s3Key)
    {
      using (var transferUtil = new TransferUtility(awsAccessKey, awsSecretKey, RegionEndpoint.USWest2))
      {
        transferUtil.Upload(stream, awsBucketName, s3Key);
      }
    }

    /// <summary>
    /// Upload a file to S3. The filepath will contain the extension matched to the content type if not already present
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

    public string GeneratePreSignedUrl(string s3Key)
    {
      var request = new GetPreSignedUrlRequest
      {
        BucketName = awsBucketName,
        Key = s3Key,
        Expires = DateTime.Now.Add(awsLinkExpiry)
      };

      using (IAmazonS3 s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USWest2))
      {
        return s3Client.GetPreSignedURL(request);
      }
    }
  }
}
