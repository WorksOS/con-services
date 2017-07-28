using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetMachineIdsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock RaptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public GetMachineIdsExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetMachineIdsExecutor()
    {
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
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
      }
      finally
      {
        //TODO: clean up
      }
      return result;

    }

    protected override void ProcessErrorCodes()
    {
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