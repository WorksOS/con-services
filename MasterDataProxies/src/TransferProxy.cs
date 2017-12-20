using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class TransferProxy : ITransferProxy
  {
    private readonly string _awsAccessKey;
    private readonly string _awsSecretKey;
    private readonly string _awsBucketName;
    public TransferProxy(IConfigurationStore configStore)
    {
      _awsAccessKey = configStore.GetValueString("AWS_ACCESS_KEY");
      _awsSecretKey = configStore.GetValueString("AWS_SECRET_KEY");
      _awsBucketName = configStore.GetValueString("AWS_BUCKET_NAME");

      if (string.IsNullOrEmpty(_awsAccessKey) || string.IsNullOrEmpty(_awsSecretKey) ||
          string.IsNullOrEmpty(_awsBucketName))
      {
        throw new Exception("Missing environment variable AWS_ACCESS_KEY, AWS_SECRET_KEY or AWS_BUCKET_NAME");
      }
    }

    public async Task<FileStreamResult> Download(string s3Key)
    {
      using (var transferUtil =
        new TransferUtility(_awsAccessKey, _awsSecretKey))
      {
        var stream = await transferUtil.OpenStreamAsync(_awsBucketName, s3Key);
        return new FileStreamResult(stream, "application/octet-stream");
      }
    }

    public void Upload(Stream stream, string s3Key)
    {
      using (var transferUtil =
        new TransferUtility(_awsAccessKey, _awsSecretKey))

      {
        transferUtil.Upload(stream, _awsBucketName, s3Key);
      }
    }
  }
}
