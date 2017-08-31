using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Velociraptor.PDSInterface.DesignProfile;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class CompactionDesignProfileExecutor<U> : RequestExecutorContainer where U : CompactionProfileVertex, new()
  {
    private CompactionProfileResult<U> PerformProductionDataProfilePost(DesignProfileProductionDataRequest request)
    {
      CompactionProfileResult<U> result;

      try
      {
        ProfilesHelper.ConvertProfileEndPositions(request.gridPoints, request.wgs84Points, out TWGS84Point startPt, out TWGS84Point endPt, out bool positionsAreGrid);
        
        var designProfile = DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args(
          request.projectId ?? -1,
          false,
          startPt,
          endPt,
          ValidationConstants.MIN_STATION,
          ValidationConstants.MAX_STATION,
          RaptorConverters.DesignDescriptor(request.designDescriptor),
          RaptorConverters.EmptyDesignDescriptor,
          RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId),
          positionsAreGrid);

        var memoryStream = raptorClient.GetDesignProfile(designProfile);

        if (memoryStream != null)
        {
          result = ConvertProfileResult(memoryStream);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to get requested slicer profile"));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      try
      {
        var profile = PerformProductionDataProfilePost(item as DesignProfileProductionDataRequest);

        if (profile != null)
        {
          result = profile;
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Failed to get requested profile calculations."));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }

    private CompactionProfileResult<U> ConvertProfileResult(MemoryStream ms)
    {
      log.LogDebug("Converting profile result");

      var profileResult = new CompactionProfileResult<U>();
      var pdsiProfile = new DesignProfile();
      pdsiProfile.ReadFromStream(ms);

      profileResult.results = pdsiProfile.vertices.ConvertAll(dpv => new U
      {
        elevation = dpv.elevation >= VelociraptorConstants.NO_HEIGHT ? float.NaN : dpv.elevation,
        station = dpv.station
      });

      ms.Close();

      profileResult.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      return profileResult;
    }
  }
}