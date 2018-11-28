using System;
using System.Collections.Generic;
using System.Linq;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetMachineIdsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      TMachineDetail[] machines = raptorClient.GetMachineIDs(request.ProjectId ?? -1);

      if (machines != null)
      {
        return MachineExecutionResult.CreateMachineExecutionResult(convertMachineStatus(machines).ToArray());
      }

      throw CreateServiceException<GetMachineIdsExecutor>();
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
