using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using System.IO;
using System.Net;
using DesignProfilerDecls;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Models;
using VLPDDecls;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from Raptor's Project/DataModel.
  /// </summary>
  /// 
  public class DesignExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// The list of imported design surfaces for a project.
    /// </summary>
    private readonly List<FileData> designs;

    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="raptorClient"></param>
    /// <param name="designs"></param>
    public DesignExecutor(ILoggerFactory logger, IASNodeClient raptorClient, List<FileData> designs) : 
      base(logger, raptorClient)
    {
      this.designs = designs;
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        log.LogDebug("Getting GeoJson design boundaries from Raptor");

        DesignBoundariesRequest request = item as DesignBoundariesRequest;

        if (request == null)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Undefined requested data DesignRequest"));
        }

        string[] geoJsons = null;

        if (designs != null && designs.Count > 0)
        {
          List<string> geoJsonList = new List<string>();

          for (var i = 0; i < designs.Count; i++)
          {
            log.LogDebug(string.Format("Getting GeoJson design boundary from Raptor for file: {0}", designs[i].Name));

            MemoryStream memoryStream;
            TDesignProfilerRequestResult designProfilerResult;
            FileDescriptor fileDescriptor = FileDescriptor.CreateFileDescriptor("", designs[i].Path, designs[i].Name);

            bool result = raptorClient.GetDesignBoundary(
              DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
                request.projectId ?? -1,
                fileDescriptor.DesignDescriptor(configStore, log, 0, 0),
                DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson,
                request.tolerance,
                TVLPDDistanceUnits.vduMeters,
                0),
              out memoryStream,
              out designProfilerResult);

            if (result)
            {
              if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null && memoryStream.Length > 0)
                geoJsonList.Add(JsonConvert.SerializeObject(memoryStream));
            }
            else
            {
              throw new ServiceException(HttpStatusCode.InternalServerError,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  string.Format("Failed to get design boundary for file: {0}", designs[i].Name)));
            }
          }

          geoJsons = geoJsonList.ToArray();
        }
        else
        {
          geoJsons = new string[0];
        }

        return DesignResult.CreateDesignResult(geoJsons); ;
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

/*
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException("Use the synchronous form of this method");
    }
*/
  }
}
