using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Velociraptor.PDSInterface.DesignProfile;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
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

        var memoryStream = this.raptorClient.GetDesignProfile(designProfile);

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
        this.ContractExecutionStates.ClearDynamic();
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
        this.ContractExecutionStates.ClearDynamic();
      }

      return result;
    }

    private CompactionProfileResult<CompactionProfileVertex> ConvertProfileResult(MemoryStream ms)
    {
      this.log.LogDebug("Converting profile result");

      var profileResult = new CompactionProfileResult<CompactionProfileVertex>();
      var pdsiProfile = new DesignProfile();
      pdsiProfile.ReadFromStream(ms);

      profileResult.results = pdsiProfile.vertices.ConvertAll(dpv => new CompactionProfileVertex
      {
        cellType = dpv.elevation >= VelociraptorConstants.NO_HEIGHT ? ProfileCellType.Gap : ProfileCellType.Edge,
        elevation = dpv.elevation >= VelociraptorConstants.NO_HEIGHT ? float.NaN : dpv.elevation,
        station = dpv.station
      });

      ms.Close();

      profileResult.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      FixGaps(profileResult.results);

      return profileResult;
    }

    /// <summary>
    /// Fixes gaps in the design profile to be consistent with how production data profile gaps are represented.
    /// </summary>
    /// <param name="results">The design profile to fix</param>
    private void FixGaps(List<CompactionProfileVertex> results)
    {
      //A gap vertex returned by Raptor is the midpoint of the gap. We need to remove any of these and set the type of
      //the previous vertex to 'Gap' to indicate the start of the gap to be consistent with production data gaps.
      if (results.Any(x => x.cellType == ProfileCellType.Gap))
      {
        //Raptor always returns the first point on the design surface even if the profile starts in a gap.
        int i = 1;
        int count = results.Count;
        while (i < count)
        {
          if (results[i].cellType == ProfileCellType.Gap)
          {
            results[i - 1].cellType = ProfileCellType.Gap;
            results.RemoveAt(i);
            count--;
          }
          else
          {
            i++;
          }
        }
      }
    }
  }
}
