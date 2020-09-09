using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeTypes;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;

namespace VSS.AWS.TransferProxy
{
  public class TransferProxy : ITransferProxy
  {
    private readonly string awsBucketName;
    private readonly TimeSpan awsLinkExpiry;
    private readonly ILogger logger;
    private readonly string awsProfile;

    private const int MAXIMUM_EXPIRY_DAYS_FOR_PRESIGNED_URL = 7;

    public TransferProxy(IConfigurationStore configStore, ILogger<TransferProxy> log, string storageKey)
    {
      logger = log;
      if (string.IsNullOrEmpty(storageKey))
      {
        throw new ArgumentException($"Missing environment variable {storageKey}", nameof(storageKey));
      }
      awsBucketName = configStore.GetValueString(storageKey);
      //AWS profile used for debugging - set value to "fsm-okta"
      awsProfile = configStore.GetValueString("AWS_PROFILE", string.Empty);

      awsLinkExpiry = configStore.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY") ??
                      TimeSpan.FromDays(MAXIMUM_EXPIRY_DAYS_FOR_PRESIGNED_URL);
    }

    private TransferUtility GetTransferUtility()
    {
      return new TransferUtility(GetS3Client());
    }

    private IAmazonS3 GetS3Client()
    {
      if (string.IsNullOrEmpty(awsProfile))
        return new AmazonS3Client(RegionEndpoint.USWest2);
      return new AmazonS3Client(new StoredProfileAWSCredentials(awsProfile), RegionEndpoint.USWest2);
    }

    public async Task<FileStreamResult> DownloadFromBucket(string s3Key, string bucketName)
    {
      using (var transferUtil = GetTransferUtility())
      {
        var stream = await transferUtil.OpenStreamAsync(bucketName, s3Key);
        string mimeType;
        try
        {
          var extension = Path.GetExtension(s3Key);

          mimeType = MimeTypeMap.GetMimeType(extension);
        }
        catch (ArgumentException)
        {
          // Unknown or invalid extension, not bad but we don't know the content type
          mimeType = ContentTypeConstants.ApplicationOctetStream; // binary data....
        }

        return new FileStreamResult(stream, mimeType);
      }
    }

    public FileStreamResult DownloadFromBucketSync(string s3Key, string bucketName)
    {
      using (var transferUtil = GetTransferUtility())
      {
        var stream = transferUtil.OpenStream(bucketName, s3Key);
        string mimeType;
        try
        {
          var extension = Path.GetExtension(s3Key);

          mimeType = MimeTypeMap.GetMimeType(extension);
        }
        catch (ArgumentException)
        {
          // Unknown or invalid extension, not bad but we don't know the content type
          mimeType = ContentTypeConstants.ApplicationOctetStream; // binary data....
        }

        return new FileStreamResult(stream, mimeType);
      }
    }
    
    /// <summary>
    /// Create a task to download a file from S3 storage
    /// </summary>
    /// <param name="s3Key">Key to the data to be downloaded</param>
    /// <returns>FileStreamResult if the file exists</returns>
    public Task<FileStreamResult> Download(string s3Key) => DownloadFromBucket(s3Key, awsBucketName);

    /// <summary>
    /// Create a task to download a file from S3 storage
    /// </summary>
    /// <param name="s3Key">Key to the data to be downloaded</param>
    /// <returns>FileStreamResult if the file exists</returns>
    public FileStreamResult DownloadSync(string s3Key) => DownloadFromBucketSync(s3Key, awsBucketName);

    public void UploadToBucket(Stream stream, string s3Key, string bucketName)
    {
      using (var transferUtil = GetTransferUtility())
      {
        transferUtil.Upload(stream, bucketName, s3Key);
      }
    }

    public void Upload(Stream stream, string s3Key)
    {
      UploadToBucket(stream, s3Key, awsBucketName);
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

      using (var s3Client = GetS3Client())
      {
        return s3Client.GetPreSignedURL(request);
      }
    }

    public bool RemoveFromBucket(string s3Key)
    {
      using (var s3Client = GetS3Client())
      {
        s3Client.DeleteObjectAsync(awsBucketName, s3Key);
        return true;
      }
    }

    /// <summary>
    /// Returns a list of keys from the S3 bucket, with a matching prefix and starting with a supplied marker key
    /// </summary>
    /// <param name="prefix">Only S3 keys with this prefix will be returned</param>
    /// <param name="maxKeys">Maximum number of keys to return</param>
    /// <param name="continuationToken">A token to supply on subsequent calls to effect a scan over larger collections of object keys. Supply null token for first call</param>
    /// <returns>A tuple containing a list of responses and a continuation token. If there are more elements to return from the query the continuation token will be a non-null, non-empty string</returns>
    public async Task<(string[], string)> ListKeys(string prefix, int maxKeys, string continuationToken = null)
    {
      using (var s3Client = GetS3Client())
      {
        var request = new ListObjectsV2Request
        {
          BucketName = awsBucketName,
          ContinuationToken = continuationToken,
          Prefix = prefix,
          MaxKeys = maxKeys
        };

        var s3Result = await s3Client.ListObjectsV2Async(request);

        return (s3Result.S3Objects.Select(x => x.Key).ToArray(), s3Result.IsTruncated ? s3Result.NextContinuationToken : string.Empty);
      }
    }

    /// <summary>
    /// Check file exists in S3 bucket
    /// </summary>
    public async Task<bool> FileExists(string s3Key)
    {
      using (var s3Client = GetS3Client())
      {

        try
        {
          ListObjectsV2Request request = new ListObjectsV2Request
          {
            BucketName = awsBucketName,
            MaxKeys = 10,
            Prefix = s3Key
          };

          ListObjectsV2Response response;
          do
          {
            response = await s3Client.ListObjectsV2Async(request);

            // Process the response.
            foreach (S3Object entry in response.S3Objects)
            {
              if (entry.Key == s3Key)
                return true;
            }
            request.ContinuationToken = response.NextContinuationToken;
          } while (response.IsTruncated);
          return false;
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
          logger.LogInformation("FileExists. S3 error occurred. Exception: " + amazonS3Exception.ToString());
        }
        catch (Exception e)
        {
          logger.LogInformation("FileExists. Exception: " + e.ToString());
        }
        return false;
      }

    }

    /// <summary>
    /// Place write delete object lock on file
    /// </summary>
    private async Task<bool> LockFile(string s3Key)
    {
      using (var s3Client = GetS3Client())
      {
        try
        {
          var res = await s3Client.PutObjectLegalHoldAsync(new PutObjectLegalHoldRequest() { BucketName = awsBucketName, Key = s3Key, LegalHold = new ObjectLockLegalHold() { Status = ObjectLockLegalHoldStatus.On }});
          return res.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Amazon.S3.AmazonS3Exception ex)
        {
          logger.LogError($"LockFile. Exception:{ex.Message}, BucketName:{awsBucketName}, s3Key:{s3Key} ");
          if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;
          //status wasn't not found, so throw the exception
          throw;
        }
      }
    }

    public async Task<bool> UploadAndLock(Stream stream, string s3Key)
    {
      UploadToBucket(stream, s3Key, awsBucketName);
      return await LockFile(s3Key);
    }


  }
}
