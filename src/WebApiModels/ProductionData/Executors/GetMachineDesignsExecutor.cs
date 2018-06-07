using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetMachineDesignsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      ProjectID request = item as ProjectID;
      TDesignName[] raptorDesigns = raptorClient.GetOnMachineDesigns(request.ProjectId ?? -1);
      if (raptorDesigns != null)
      {
        //For details, need to set the end dates so can test date range
        var designDetails = new List<DesignName>();

        var machineIds = raptorDesigns.Select(d => d.FMachineID).Distinct();
        foreach (var machineId in machineIds)
        {
          var machineDesigns = raptorDesigns.Where(d => d.FMachineID == machineId).OrderBy(d => d.FStartDate.ToDateTime()).ToList();
          for (int i = 1; i < machineDesigns.Count; i++)
          {
            designDetails.Add(DesignName.CreateDesignNames(
              machineDesigns[i - 1].FName, machineDesigns[i - 1].FID, machineDesigns[i - 1].FMachineID, machineDesigns[i - 1].FStartDate, machineDesigns[i].FStartDate));
          }
          designDetails.Add(DesignName.CreateDesignNames(
            machineDesigns[machineDesigns.Count - 1].FName, machineDesigns[machineDesigns.Count - 1].FID, machineDesigns[machineDesigns.Count - 1].FMachineID, machineDesigns[machineDesigns.Count - 1].FStartDate, DateTime.UtcNow));
        }
        
        result =
          MachineDesignsExecutionResult.CreateMachineExecutionResult(designDetails);
      }
      else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to get requested machines designs details"));

      return result;
    }
  }
}
