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
using DesignProfilerDecls;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Models;
using VLPDDecls;
using VSS.MasterData.Models.Models;

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
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      log.LogDebug("Getting GeoJson design boundaries from Raptor");

      DesignBoundariesRequest request = item as DesignBoundariesRequest;

      string geoJson = "";

      foreach (var design in designs)
      {
        MemoryStream memoryStream;
        TDesignProfilerRequestResult designProfilerResult;

        raptorClient.GetDesignBoundary(
          DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
            request.projectId.Value, 
            DesignDescriptor(0, FileDescriptor.CreateFileDescriptor("", design.Path, design.Name), 0), 
            DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson, 
            request.tolerance,
            TVLPDDistanceUnits.vduMeters,
            0), 
          out memoryStream, 
          out designProfilerResult);

        if (memoryStream != null && memoryStream.Length > 0)
        {
          geoJson = string.Format("{0},", JsonConvert.SerializeObject(memoryStream));
        }
      }

      return null;
    }

    /// <summary>
    /// Creates a Raptor design file descriptor
    /// </summary>
    /// <param name="designId">The id of the design file</param>
    /// <param name="fileDescr">The location and name of the design file</param>
    /// <param name="offset">The offset if the file is a reference surface</param>
    /// <returns></returns>
    private TVLPDDesignDescriptor DesignDescriptor(long designId, FileDescriptor fileDescr, double offset)
    {
      string filespaceName = configStore.GetValueString("TCCFILESPACENAME");

      if (string.IsNullOrEmpty(filespaceName))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACENAME";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      return VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(designId, filespaceName, fileDescr.filespaceId, fileDescr.path, fileDescr.fileName, offset);
    }

  }
}
