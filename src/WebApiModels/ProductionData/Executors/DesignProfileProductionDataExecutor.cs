using System;
using System.IO;
using System.Net;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class DesignProfileProductionDataExecutor : RequestExecutorContainer
  {
    private ProfileResult PerformProductionDataProfilePost(ProfileProductionDataRequest request)
    {
      ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out TWGS84Point startPt, out TWGS84Point endPt, out bool positionsAreGrid);

      var designProfile = DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args(
        request.projectId ?? -1,
        false,
        startPt,
        endPt,
        ValidationConstants.MIN_STATION,
        ValidationConstants.MAX_STATION,
        RaptorConverters.DesignDescriptor(request.alignmentDesign),
        RaptorConverters.EmptyDesignDescriptor,
        null,
        positionsAreGrid);

      var memoryStream = raptorClient.GetDesignProfile(designProfile);


      return memoryStream != null
        ? ProfilesHelper.convertProductionDataProfileResult(memoryStream, request.callId ?? Guid.NewGuid())
        : null; // TODO: return appropriate result
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      try
      {
        ProfileResult profile = PerformProductionDataProfilePost(item as ProfileProductionDataRequest);

        if (profile != null)
          result = profile;
        else
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Failed to get requested profile calculations."));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }
  }
}