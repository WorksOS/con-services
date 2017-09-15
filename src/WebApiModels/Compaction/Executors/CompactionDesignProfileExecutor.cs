using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Velociraptor.PDSInterface.DesignProfile;

namespace VSS.Productivity3D.WebApiModels.Compaction.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class CompactionDesignProfileExecutor : RequestExecutorContainer 
  {
    private CompactionProfileResult<CompactionProfileVertex> PerformProductionDataProfilePost(CompactionProfileDesignRequest request)
    {
      CompactionProfileResult<CompactionProfileVertex> result;

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
          //For convenience return empty list rather than null for easier manipulation
          result = new CompactionProfileResult<CompactionProfileVertex>{results = new List<CompactionProfileVertex>()};
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
        var profile = PerformProductionDataProfilePost(item as CompactionProfileDesignRequest);

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

    private CompactionProfileResult<CompactionProfileVertex> ConvertProfileResult(MemoryStream ms)
    {
      log.LogDebug("Converting profile result");

      var profileResult = new CompactionProfileResult<CompactionProfileVertex>();
      var pdsiProfile = new DesignProfile();
      pdsiProfile.ReadFromStream(ms);

      profileResult.results = pdsiProfile.vertices.ConvertAll(dpv => new CompactionProfileVertex
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