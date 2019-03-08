using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
#if RAPTOR
using VLPDDecls;
using VSS.Velociraptor.PDSInterface.DesignProfile;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class CompactionDesignProfileExecutor : RequestExecutorContainer 
  {
#if RAPTOR
    private CompactionProfileResult<CompactionProfileVertex> PerformProductionDataProfilePost(CompactionProfileDesignRequest request)
    {
      CompactionProfileResult<CompactionProfileVertex> result;

      try
      {
        ProfilesHelper.ConvertProfileEndPositions(request.GridPoints, request.WGS84Points, out TWGS84Point startPt, out TWGS84Point endPt, out bool positionsAreGrid);
        
        var designProfile = DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args(
          request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          false,
          startPt,
          endPt,
          ValidationConstants3D.MIN_STATION,
          ValidationConstants3D.MAX_STATION,
          RaptorConverters.DesignDescriptor(request.DesignDescriptor),
          RaptorConverters.EmptyDesignDescriptor,
          RaptorConverters.ConvertFilter(request.Filter),
          positionsAreGrid);

        var memoryStream = raptorClient.GetDesignProfile(designProfile);

        if (memoryStream != null)
        {
          result = ConvertProfileResult(memoryStream);
          memoryStream.Close();
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

    private CompactionProfileResult<CompactionProfileVertex> ConvertProfileResult(MemoryStream ms)
    {
      log.LogDebug("Converting profile result");

      var profileResult = new CompactionProfileResult<CompactionProfileVertex>();
      var pdsiProfile = new DesignProfile();
      pdsiProfile.ReadFromStream(ms);

      profileResult.results = pdsiProfile.vertices.ConvertAll(dpv => new CompactionProfileVertex
      {
        cellType = dpv.elevation >= VelociraptorConstants.NO_HEIGHT ? ProfileCellType.Gap : ProfileCellType.Edge,
        elevation = dpv.elevation >= VelociraptorConstants.NO_HEIGHT ? float.NaN : dpv.elevation,
        station = dpv.station
      });

      profileResult.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      FixGaps(profileResult.results);

      return profileResult;
    }
#endif

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CompactionProfileDesignRequest>(item);

        var profile =
#if RAPTOR
            UseTRexGateway("ENABLE_TREX_GATEWAY_PROFILING") ?
#endif
              await PerformProductionDataProfilePostWithTRexGateway(request)
#if RAPTOR
              : PerformProductionDataProfilePost(request)
#endif
        ;

        if (profile != null)
          return profile;

        throw CreateServiceException<CompactionDesignProfileExecutor>();
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private async Task<CompactionProfileResult<CompactionProfileVertex>> PerformProductionDataProfilePostWithTRexGateway(CompactionProfileDesignRequest request)
    {
      ProfilesHelper.ConvertProfileEndPositions(request.GridPoints, request.WGS84Points, out WGSPoint startPt, out var endPt);

      var designProfileRequest = new DesignProfileRequest(request.ProjectUid ?? Guid.Empty, request.DesignDescriptor?.FileUid ?? Guid.Empty, startPt.Lon, startPt.Lat, endPt.Lon, endPt.Lat);

      var trexResult = await trexCompactionDataProxy.SendDataPostRequest<DesignProfileResult, DesignProfileRequest>(designProfileRequest, "/profile/design", customHeaders);

      return trexResult != null && trexResult.HasData() ? ConvertTRexProfileResult(trexResult) : null;
    }

    private CompactionProfileResult<CompactionProfileVertex> ConvertTRexProfileResult(DesignProfileResult profile)
    {
      log.LogDebug("Converting TRex profile result");

      var profileResult = new CompactionProfileResult<CompactionProfileVertex>();

      profileResult.results = profile.ProfileLine.ConvertAll(dpv => new CompactionProfileVertex
      {
        cellType = dpv.Z >= VelociraptorConstants.NO_HEIGHT ? ProfileCellType.Gap : ProfileCellType.Edge,
        elevation = dpv.Z >= VelociraptorConstants.NO_HEIGHT ? float.NaN : (float) dpv.Z,
        station = dpv.Station
      });

      profileResult.gridDistanceBetweenProfilePoints =
        profile.ProfileLine.Count > 1 ? profile.ProfileLine[profile.ProfileLine.Count - 1].Station - profile.ProfileLine[0].Station : 0;
      
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

  }
}
