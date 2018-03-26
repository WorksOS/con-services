using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetMachineIdsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      ProjectID request = item as ProjectID;
      TMachineDetail[] machines = raptorClient.GetMachineIDs(request.projectId ?? -1);
      if (machines != null)
        result =
          MachineExecutionResult.CreateMachineExecutionResult(
            convertMachineStatus(machines).ToArray());
      else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to get requested machines details"));

      return result;
    }

    private IEnumerable<MachineStatus> convertMachineStatus(TMachineDetail[] machines)
    {
      foreach (TMachineDetail machineDetail in machines)
        yield return
            MachineStatus.CreateMachineStatus(
            machineDetail.ID,
            machineDetail.Name,
            machineDetail.IsJohnDoeMachine,
            string.IsNullOrEmpty(machineDetail.LastKnownDesignName) ? null : machineDetail.LastKnownDesignName,
            machineDetail.LastKnownLayerId == 0 ? (ushort?)null : machineDetail.LastKnownLayerId,
            machineDetail.LastKnownTimeStamp.ToDateTime() == ConversionConstants.PDS_MIN_DATE ? (DateTime?)null : machineDetail.LastKnownTimeStamp.ToDateTime(),
            machineDetail.LastKnownLat == 0 ? (double?)null : machineDetail.LastKnownLat,
            machineDetail.LastKnownLon == 0 ? (double?)null : machineDetail.LastKnownLon,
            machineDetail.LastKnownX == 0 ? (double?)null : machineDetail.LastKnownX,
            machineDetail.LastKnownY == 0 ? (double?)null : machineDetail.LastKnownY
            );
    }
  }
}