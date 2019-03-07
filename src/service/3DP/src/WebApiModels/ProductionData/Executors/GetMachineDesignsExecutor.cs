using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetMachineDesignsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation($"GetMachineDesignsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")}");

      MachineDesignsExecutionResult machineDesignsResult = null;


#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS"))
#endif
      {
        if (request.ProjectUid.HasValue && request.ProjectUid != Guid.Empty)
        {
          var siteModelId = request.ProjectUid.ToString();

          machineDesignsResult = trexCompactionDataProxy
            .SendDataGetRequest<MachineDesignsExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/designs",
              customHeaders)
            .Result;
          // todoJeannie pair machineUids and designUids
          return machineDesignsResult;
        }
      }

#if RAPTOR
      if (request.ProjectId.HasValue && request.ProjectId >= 1)
      {
        var raptorDesigns = raptorClient.GetOnMachineDesignEvents(request.ProjectId ?? -1);

        if (raptorDesigns != null)
        {
          machineDesignsResult = CreateResultFromTDesignName(raptorDesigns);
          // todoJeannie pair machineUids and designUids
          return machineDesignsResult;
        }
      }
#endif

      throw CreateServiceException<GetMachineDesignsExecutor>();
    }

#if RAPTOR
    private MachineDesignsExecutionResult CreateResultFromTDesignName(TDesignName[] raptorDesigns)
    {
      //For details, need to set the end dates so can test date range
      var designDetails = new List<DesignName>();
      var machineIds = raptorDesigns.Select(d => d.FMachineID).Distinct();

      foreach (var machineId in machineIds)
      {
        var machineDesigns = raptorDesigns.Where(d => d.FMachineID == machineId).OrderBy(d => d.FStartDate.ToDateTime())
          .ToList();
        for (var i = 1; i < machineDesigns.Count; i++)
        {
          designDetails.Add(DesignName.CreateDesignNames(
            machineDesigns[i - 1].FName,
            machineDesigns[i - 1].FID,
            machineDesigns[i - 1].FMachineID,
            machineDesigns[i - 1].FStartDate,
            machineDesigns[i].FStartDate));
        }

        designDetails.Add(DesignName.CreateDesignNames(
          machineDesigns[machineDesigns.Count - 1].FName,
          machineDesigns[machineDesigns.Count - 1].FID,
          machineDesigns[machineDesigns.Count - 1].FMachineID,
          machineDesigns[machineDesigns.Count - 1].FStartDate,
          DateTime.UtcNow));
      }

      return new MachineDesignsExecutionResult(designDetails);
    }
#endif

    private MachineDesignsExecutionResult CreateResultFromMachineDesignEvents(MachineDesignEventsResult tRexDesigns)
    {
      //For details, need to set the end dates so can test date range
      var designDetails = new List<DesignName>();

      //todoJeannie
      //var machineIds = raptorDesigns.Select(d => d.FMachineID).Distinct();

      //foreach (var machineId in machineIds)
      //{
      //  var machineDesigns = raptorDesigns.Where(d => d.FMachineID == machineId).OrderBy(d => d.FStartDate.ToDateTime())
      //    .ToList();
      //  for (var i = 1; i < machineDesigns.Count; i++)
      //  {
      //    designDetails.Add(DesignName.CreateDesignNames(
      //      machineDesigns[i - 1].FName,
      //      machineDesigns[i - 1].FID,
      //      machineDesigns[i - 1].FMachineID,
      //      machineDesigns[i - 1].FStartDate,
      //      machineDesigns[i].FStartDate));
      //  }

      //  designDetails.Add(DesignName.CreateDesignNames(
      //    machineDesigns[machineDesigns.Count - 1].FName,
      //    machineDesigns[machineDesigns.Count - 1].FID,
      //    machineDesigns[machineDesigns.Count - 1].FMachineID,
      //    machineDesigns[machineDesigns.Count - 1].FStartDate,
      //    DateTime.UtcNow));
      //}

      //return MachineDesignsExecutionResult.Create(designDetails);
      return new MachineDesignsExecutionResult(designDetails);
    }

  }
}
