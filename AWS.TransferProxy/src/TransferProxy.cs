using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<FileStreamResult> Download(string s3Key)
    {
      using (var transferUtil = new TransferUtility(awsAccessKey, awsSecretKey, RegionEndpoint.USWest2))
      {
        var stream = await transferUtil.OpenStreamAsync(awsBucketName, s3Key);
        return new FileStreamResult(stream, "application/octet-stream");
      }
    }

    public void Upload(Stream stream, string s3Key)
    {
      using (var transferUtil = new TransferUtility(awsAccessKey, awsSecretKey, RegionEndpoint.USWest2))
      {
        transferUtil.Upload(stream, awsBucketName, s3Key);
      }
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
