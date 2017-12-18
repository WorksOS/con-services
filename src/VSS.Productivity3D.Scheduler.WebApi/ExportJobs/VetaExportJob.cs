using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Transfer;
using Hangfire.Server;
using Microsoft.AspNetCore.Mvc;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  public class VetaExportJob
  {
    public void ExportDataToVeta(IRaptorProxy raptor, IConfigurationStore config, Guid projectUid,
      string fileName, string machineNames, Guid? filterUid, IDictionary<string, string> customHeaders,
      PerformContext context)
    {
      var data = raptor.GetVetaExportData(projectUid, fileName, machineNames, filterUid, customHeaders).Result;
      using (var transferUtil =
        new TransferUtility(config.GetValueString("AWS_ACCESS_KEY"), config.GetValueString("AWS_SECRET_KEY"))
      )
      {
        transferUtil.Upload(new MemoryStream(data.Data), config.GetValueString("AWS_BUCKET_NAME"),
          GetS3Key(projectUid, context.BackgroundJob.Id));
      }
    }

    public static string GetS3Key(Guid project, string jobId)
    {
      return $"{project.ToString()}/{jobId}";
    }
  }
}
