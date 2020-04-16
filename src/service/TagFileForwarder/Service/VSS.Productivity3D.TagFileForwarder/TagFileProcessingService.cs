using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileForwarder
{
  public class TagFileProcessingService : IHostedService
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITagFileForwarder _tagFileForwarder;
    private readonly IConfigurationStore _config;
    private readonly ILogger<TagFileProcessingService> _logger;

    private string _bucketName;

    private Task _task;

    private CancellationTokenSource _cts;

    public TagFileProcessingService(IServiceScopeFactory scopeFactory, ITagFileForwarder tagFileForwarder, IConfigurationStore config, ILogger<TagFileProcessingService> logger)
    {
      _scopeFactory = scopeFactory;
      _tagFileForwarder = tagFileForwarder;
      _logger = logger;
      _config = config;
      _bucketName = _config.GetValueString("AWS_ALL_TAGFILE_BUCKET_NAME");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      _task = StartProcessingTagFiles(_cts.Token);

      return _task.IsCompleted ? Task.CompletedTask : _task;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      if (_task == null)
      {
        return;
      }

      _cts.Cancel();

      await Task.WhenAny(_task, Task.Delay(-1, cancellationToken));

      cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task StartProcessingTagFiles(CancellationToken token)
    {

      var profile = _config.GetValueString("AWS_PROFILE", null);
      var s3Client = string.IsNullOrEmpty(profile) 
        ? new AmazonS3Client(RegionEndpoint.USWest2) 
        : new AmazonS3Client(new StoredProfileAWSCredentials(profile), RegionEndpoint.USWest2);

      var startDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

      var now = DateTime.UtcNow;

      while (startDate < now)
      {
        await ProcessDate(startDate, s3Client, token);
        startDate = startDate.AddDays(1);
      }

      while (true)
      {
        // Indefinite processing
        await ProcessDate(DateTime.Now, s3Client, token);
        await Task.Delay(new TimeSpan(0, 5, 0), token); // 5 min delay
      }
    }

    private async Task ProcessDate(DateTime dateTimeUtc, IAmazonS3 s3Client, CancellationToken token)
    {
      _logger.LogInformation($"Processing Tag Files for {dateTimeUtc.ToLongDateString()}");
      var listRequest = new ListObjectsV2Request
      {
        BucketName = _bucketName,
        Prefix = $"{dateTimeUtc:yyyy-MM-dd}/" // same as the tag file dump
      };

      ListObjectsV2Response listResponse;
      do
      {
        listResponse = await s3Client.ListObjectsV2Async(listRequest, token);
        foreach (var obj in listResponse.S3Objects)
        {
          _logger.LogInformation($"Attempting to send the tag file: {obj.Key}");

          if (GetS3Key(obj.Key, out var method, out var tagFileName, out var tccOrg))
          {
            await using var memoryStream = new MemoryStream();
            using var transferUtil = new TransferUtility(s3Client);

            var stream = await transferUtil.OpenStreamAsync(_bucketName, obj.Key, token);
            stream.CopyTo(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var request = new CompactionTagFileRequest()
            {
              Data = memoryStream.ToArray(),FileName = tagFileName, OrgId = tccOrg
            };

            _logger.LogInformation($"Found Filename: {tagFileName}, method: {method}, TCC OrgID: {tccOrg}");

            var result = await _tagFileForwarder.SendTagFileDirect(request, null);

            if (result == null || result.Code != 0)
            {
              _logger.LogWarning($"Failed to process tag file {tagFileName}. Code: {result?.Code}. Message: {result?.Message}");
            }
            else
            {
              var renameName = $"completed/{obj.Key}";
              _logger.LogInformation($"Processed tag file {tagFileName} successfully");

              // S3 does not allow files to be renamed, you need to copy and delete
              var copyResult = await s3Client.CopyObjectAsync(_bucketName, obj.Key, _bucketName, renameName, token);
              if (copyResult.HttpStatusCode == HttpStatusCode.OK)
              {
                var deleteResponse = await s3Client.DeleteObjectAsync(_bucketName, obj.Key, token);
                if (deleteResponse.HttpStatusCode == HttpStatusCode.NoContent)
                {
                  _logger.LogInformation($"Moved Tagfile to {renameName}");
                }
                else
                {
                  _logger.LogWarning($"Failed to rename (delete old) file {renameName}, http code: {deleteResponse.HttpStatusCode}");
                }
              }
              else
              {
                _logger.LogWarning($"Failed to rename (copy old) file {renameName}, http code: {copyResult.HttpStatusCode}");
              }
            }
          }
          else
          {
            _logger.LogError($"Failed to parse S3 Key: {obj.Key}");
          }
          
        }

        listRequest.ContinuationToken = listResponse.NextContinuationToken;
      } while (listResponse.IsTruncated);
    }


    private bool GetS3Key(string key, out string method, out string tagFileName, out string tccOrgId)
    {
      //Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
      //Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
      //Required folder structure is <TCC org id>/<serial>--<machine name>/<archive folder>/<serial--machine name--date>/<tagfile>
      //e.g. 0415J010SW--HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
      
      // Exisitng logic
//      const string separator = "--";
//      string[] parts = tagFileName.Split(new string[] {separator}, StringSplitOptions.None);
//      var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);
//      //TCC org ID is not provided with direct submission from machines
//      var prefix = string.IsNullOrEmpty(tccOrgId) ? string.Empty : $"{tccOrgId}/";
//      return $"{DateTime.Today:yyyy-MM-dd}/{method}/{prefix}{parts[0]}{separator}{parts[1]}/{nameWithoutTime}/";

      method = null;
      tagFileName = null;
      tccOrgId = null;

      var items = key.Split("/");
      if (items.Length < 4)
        return false;

      method = items[1];
      if (items.Length == 5)
      {
        tagFileName = items[4];
        tccOrgId = null;
        return true;
      }
      else if (items.Length == 6)
      {
        tagFileName = items[5];
        tccOrgId = items[2];
        return true;
      }
      
      return false;


    }



  }
}
