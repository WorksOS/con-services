using System;
using System.Collections.Generic;
using System.IO;
using Hangfire.Server;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for managing a veta export job
  /// </summary>
  public class VetaExportJob : IVetaExportJob
  {
    private IRaptorProxy _raptor;
    private ITransferProxy _transferProxy;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="raptor"></param>
    /// <param name="transferProxy"></param>
    public VetaExportJob(IRaptorProxy raptor, ITransferProxy transferProxy)
    {
      _raptor = raptor;
      _transferProxy = transferProxy;
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
      _transferProxy.Upload(new MemoryStream(data.Data), GetS3Key(projectUid, context.BackgroundJob.Id));
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
