using SVOICOptionsDecls;
using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApiModels.ProductionData.Helpers;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class ProfileProductionDataExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProfileProductionDataExecutor()
    {
      ProcessErrorCodes();
    }
    private ProfileResult performProductionDataProfilePost(ProfileProductionDataRequest request)
    {
      MemoryStream ms = null;

      if (!RaptorConverters.DesignDescriptor(request.alignmentDesign).IsNull())
      {
        ASNode.RequestAlignmentProfile.RPC.TASNodeServiceRPCVerb_RequestAlignmentProfile_Args args
             = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args
             (request.projectId ?? -1,
              -1, // don't care
              request.startStation ?? ValidationConstants.MIN_STATION,
              request.endStation ?? ValidationConstants.MIN_STATION,
              RaptorConverters.DesignDescriptor(request.alignmentDesign),
              RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId, null, null),
              RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
              RaptorConverters.DesignDescriptor(request.alignmentDesign),
              request.returnAllPassesAndLayers);

        ms = raptorClient.GetAlignmentProfile(args);
      }
      else
      {
        VLPDDecls.TWGS84Point startPt, endPt;
        bool positionsAreGrid;
        ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);

        ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
             = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
             (request.projectId ?? -1,
              -1, // don't care
              positionsAreGrid,
              startPt,
              endPt,
              RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId, null, null),
              RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
              RaptorConverters.DesignDescriptor(request.alignmentDesign),
              request.returnAllPassesAndLayers);

        ms = raptorClient.GetProfile(args);
      }

      if (ms != null)
      {
        return ProfilesHelper.convertProductionDataProfileResult(ms, (Guid)(request.callId ?? Guid.NewGuid()));
      }
      else
      {
        // TODO: return appropriate result
        return null;
      }
    }

      protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        ProfileResult profile = performProductionDataProfilePost(item as ProfileProductionDataRequest);

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