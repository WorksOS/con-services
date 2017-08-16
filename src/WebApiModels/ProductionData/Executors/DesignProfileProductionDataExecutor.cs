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
using VSS.Productivity3D.WebApiModels.Extensions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class DesignProfileProductionDataExecutor : RequestExecutorContainer
  {
    private const int PROFILE_TYPE_NOT_REQUIRED = -1;

    private ProfileResult PerformProductionDataProfilePost(ProfileProductionDataRequest request)
    {
      MemoryStream memoryStream = null;

      if (!RaptorConverters.DesignDescriptor(request.alignmentDesign).IsNull())
      {
        var designDescriptor = __Global.Construct_TVLPDDesignDescriptor(
          request.alignmentDesign.id,
          "RaptorServices",
          request.alignmentDesign.file.filespaceId,
          request.alignmentDesign.file.path,
          request.alignmentDesign.file.fileName,
          request.alignmentDesign.offset);

        ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out TWGS84Point startPt, out TWGS84Point endPt, out bool positionsAreGrid);

        var designProfile = DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args(
          request.projectId.Value,
          false,
          startPt,
          endPt,
          ValidationConstants.MIN_STATION,
          ValidationConstants.MAX_STATION,
          designDescriptor,
          RaptorConverters.EmptyDesignDescriptor,
          null,
          positionsAreGrid);

        //var tmp = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args(
        //  request.projectId ?? -1,
        //  PROFILE_TYPE_NOT_REQUIRED,
        //  positionsAreGrid,
        //  startPt,
        //  endPt,
        //  RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId),
        //  RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
        //  RaptorConverters.DesignDescriptor(request.alignmentDesign),
        //  request.returnAllPassesAndLayers);


        memoryStream = raptorClient.GetDesignProfile(designProfile);
      }
      //else
      //{

      //var args = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args(
      //  request.projectId ?? -1,
      //      PROFILE_TYPE_NOT_REQUIRED,
      //      positionsAreGrid,
      //      startPt,
      //      endPt,
      //      RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId),
      //      RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
      //      RaptorConverters.DesignDescriptor(request.alignmentDesign),
      //      request.returnAllPassesAndLayers);

      //  memoryStream = raptorClient.GetProfile(args);
      //}

      if (memoryStream != null)
      {
        return ProfilesHelper.convertProductionDataProfileResult(memoryStream, request.callId ?? Guid.NewGuid());
      }

      // TODO: return appropriate result
      return null;
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