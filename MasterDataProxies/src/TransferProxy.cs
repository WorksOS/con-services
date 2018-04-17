using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class TransferProxy : ITransferProxy
  {
    private readonly string _awsAccessKey;
    private readonly string _awsSecretKey;
    private readonly string _awsBucketName;
    private readonly TimeSpan _awsLinkExpiry;

    public TransferProxy(IConfigurationStore configStore)
    {
      _awsAccessKey = configStore.GetValueString("AWS_ACCESS_KEY");
      _awsSecretKey = configStore.GetValueString("AWS_SECRET_KEY");
      _awsBucketName = configStore.GetValueString("AWS_BUCKET_NAME");
      //Maxium expiry for presigned url's is a week
      _awsLinkExpiry = configStore.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY") ?? TimeSpan.FromDays(7);

      if (string.IsNullOrEmpty(_awsAccessKey) || string.IsNullOrEmpty(_awsSecretKey) ||
          string.IsNullOrEmpty(_awsBucketName))
      {
        throw new Exception("Missing environment variable AWS_ACCESS_KEY, AWS_SECRET_KEY or AWS_BUCKET_NAME");
      }
    }

    public async Task<FileStreamResult> Download(string s3Key)
    {
      using (var transferUtil =
        new TransferUtility(_awsAccessKey, _awsSecretKey, RegionEndpoint.USWest2))
      {
        var stream = await transferUtil.OpenStreamAsync(_awsBucketName, s3Key);
        return new FileStreamResult(stream, "application/octet-stream");
      }
    }

    public void Upload(Stream stream, string s3Key)
    {
      using (var transferUtil =
        new TransferUtility(_awsAccessKey, _awsSecretKey, RegionEndpoint.USWest2))

      {
        transferUtil.Upload(stream, _awsBucketName, s3Key);
      }
    }

    public string GeneratePreSignedUrl(string s3Key)
    {
      GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
      {
        BucketName = _awsBucketName,
        Key = s3Key,
        Expires = DateTime.Now.Add(_awsLinkExpiry)
      };
      using (IAmazonS3 s3Client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, RegionEndpoint.USWest2))
      {
        return s3Client.GetPreSignedURL(request);
      }
    }
  }
}
