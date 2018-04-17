using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.IO;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for managing a veta export job
  /// </summary>
  public class VetaExportJob : IVetaExportJob
  {
    private readonly IRaptorProxy raptor;
    private readonly ITransferProxy transferProxy;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="raptor"></param>
    /// <param name="transferProxy"></param>
    public VetaExportJob(IRaptorProxy raptor, ITransferProxy transferProxy)
    {
      this.raptor = raptor;
      this.transferProxy = transferProxy;
    }

    /*
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
      var data = raptor.GetVetaExportData(projectUid, fileName, machineNames, filterUid, customHeaders).Result;
      transferProxy.Upload(new MemoryStream(data.ExportData), GetS3Key(customHeaders["X-VisionLink-CustomerUid"], projectUid, context.BackgroundJob.Id));
    }

  */
    /// <summary>
    /// Gets the S3 key for a job
    /// </summary>
    /// <param name="customerUid">The customer Uid</param>
    /// <param name="projectUid">The project Uid</param>
    /// <param name="jobId">The job id</param>
    /// <returns>The S3 key where the file is stored. This is the full path and file name in AWS.</returns>
    public static string GetS3Key(string customerUid, Guid projectUid, string jobId)
    {
      return $"3dpm/{customerUid}/{projectUid}/{jobId}.zip";
    }
  }
}
