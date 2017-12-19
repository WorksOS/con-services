using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3.Transfer;
using Hangfire.Server;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for managing a veta export job
  /// </summary>
  public class VetaExportJob : IVetaExportJob
  {
    private IRaptorProxy _raptor;
    private IConfigurationStore _config;

    private readonly string _awsAccessKey;
    private readonly string _awsSecretKey;
    private readonly string _awsBucketName;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="raptor"></param>
    /// <param name="config"></param>
    public VetaExportJob(IRaptorProxy raptor, IConfigurationStore config)
    {
      _raptor = raptor;
      _config = config;
      _awsAccessKey = _config.GetValueString("AWS_ACCESS_KEY");
      _awsSecretKey = _config.GetValueString("AWS_SECRET_KEY");
      _awsBucketName = _config.GetValueString("AWS_BUCKET_NAME");

      if (string.IsNullOrEmpty(_awsAccessKey) || string.IsNullOrEmpty(_awsSecretKey) ||
          string.IsNullOrEmpty(_awsBucketName))
      {
        throw new Exception("Missing environment variable AWS_ACCESS_KEY, AWS_SECRET_KEY or AWS_BUCKET_NAME");
      }

    }

    /// <summary>
    /// Gets the veta export data from 3dpm and saves it to AWS S3.
    /// </summary>
    /// <param name="projectUid">The project Uid</param>
    /// <param name="fileName">The name of the file</param>
    /// <param name="machineNames">The machine names</param>
    /// <param name="filterUid">The filter Uid</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="context">Hangfire context</param>
    public void ExportDataToVeta(Guid projectUid, string fileName, string machineNames, Guid? filterUid, IDictionary<string, string> customHeaders,
      PerformContext context)
    {
      var data = _raptor.GetVetaExportData(projectUid, fileName, machineNames, filterUid, customHeaders).Result;

      using (var transferUtil =
        new TransferUtility(_awsAccessKey, _awsSecretKey))

      {
        transferUtil.Upload(new MemoryStream(data.Data), _awsBucketName, GetS3Key(projectUid, context.BackgroundJob.Id));
      }
    }

    /// <summary>
    /// Gets the S3 key for a job
    /// </summary>
    /// <param name="projectUid">The project Uid</param>
    /// <param name="jobId">The job id</param>
    /// <returns>Tee S3 key where the file is stored</returns>
    public static string GetS3Key(Guid projectUid, string jobId)
    {
      return $"{projectUid}/{jobId}";
    }
  }
}
