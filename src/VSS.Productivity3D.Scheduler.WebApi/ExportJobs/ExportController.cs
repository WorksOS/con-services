using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  public class ExportController : Controller
  {
    private IRaptorProxy raptorProxy;
    private IConfigurationStore configStore;

    public ExportController(IRaptorProxy raptor, IConfigurationStore config)
    {
      raptorProxy = raptor;
      configStore = config;
    }
    

    [Route("api/v1/export/veta")]
    [HttpGet]
    public string StartVetaExport(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      return BackgroundJob.Enqueue(() => new VetaExportJob().ExportDataToVeta(raptorProxy, configStore,
        projectUid, fileName, machineNames, filterUid, Request.Headers.GetCustomHeaders(), null));
    }
    
    [Route("api/v1/export/veta/{projectUid}/{jobId}")]
    [HttpGet]
    public Tuple<string,string> GetVetaExportStatus(Guid projectUid, string jobId)
    {
      return new Tuple<string, string>(VetaExportJob.GetS3Key(projectUid, jobId),
        JobStorage.Current.GetMonitoringApi().JobDetails(jobId).History.LastOrDefault()?.StateName);
    }

  }
}
