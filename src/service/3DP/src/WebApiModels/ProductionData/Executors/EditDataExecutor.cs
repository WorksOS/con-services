using System;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ShineOn.Rtl;
using TAGProcServiceDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class EditDataExecutor : BaseEditDataExecutor
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public EditDataExecutor()
    {
#if RAPTOR
      ProcessErrorCodes();
#endif
    }
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<EditDataRequest>(item);
        //Note: request.dataEdit should only be null for a global undo. This is checked in request model validation
        //so the following should never happen. But just in case...
        if (request.dataEdit == null && !request.undo)
          return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "No data edit to perform");

#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_EDIT_DATA"))
        {
#endif
          var projectIds = new ProjectIDs(request.ProjectId.Value, request.ProjectUid.Value);
          projectIds.Validate();
          var assetUid = await GetAssetUid(projectIds, request.dataEdit.assetId);

          var trexRequest = new TRexEditData(assetUid ?? Guid.Empty, request.dataEdit.startUTC, request.dataEdit.endUTC, request.dataEdit.onMachineDesignName, request.dataEdit.liftNumber);
          var trexResult = await trexCompactionDataProxy.SendDataDeleteRequest<ContractExecutionResult, TRexEditData>(trexRequest, "/productiondataedit", customHeaders, true);
          if (trexResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
            throw new ServiceException(HttpStatusCode.BadRequest,trexResult);

          return trexResult;
#if RAPTOR
        }

        return ProcessWithRaptor(request);
#endif

      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(EditDataRequest request)
    {
      TDateTime startTime;
      DateTime endTime;
      if (request.dataEdit == null)
      {
        startTime = new TDateTime();
        endTime = new DateTime();
      }
      else
      {
        var st = request.dataEdit.startUTC;
        startTime = new TDateTime((ushort)st.Year, (ushort)st.Month, (ushort)st.Day, (ushort)st.Hour,
            (ushort)st.Minute, (ushort)st.Second, (ushort)st.Millisecond);
        var et = request.dataEdit.endUTC;
        endTime = new TDateTime((ushort)et.Year, (ushort)et.Month, (ushort)et.Day, (ushort)et.Hour,
            (ushort)et.Minute, (ushort)et.Second, (ushort)et.Millisecond);
      }
      var returnResult = TTAGProcServerProcessResult.tpsprOK;
      var tagClient = tagProcessor.ProjectDataServerTAGProcessorClient();
      if (request.undo)
      {
        if (request.dataEdit == null)
        {
          returnResult = tagClient.SubmitOverrideDesignRemove(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, -1, new TDateTime());
          if (returnResult == TTAGProcServerProcessResult.tpsprOK)
            returnResult = tagClient.SubmitOverrideLayerRemove(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, -1, new TDateTime());
        }
        else
        {
          if (!string.IsNullOrEmpty(request.dataEdit.onMachineDesignName))
          {
            returnResult = tagClient.SubmitOverrideDesignRemove(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.dataEdit.assetId,
                startTime);
          }
          if (request.dataEdit.liftNumber.HasValue && returnResult == TTAGProcServerProcessResult.tpsprOK)
          {
            returnResult = tagClient.SubmitOverrideLayerRemove(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.dataEdit.assetId,
                startTime);
          }
        }
      }
      else
      {
        if (!string.IsNullOrEmpty(request.dataEdit.onMachineDesignName))
        {
          //Machine design
          returnResult = tagClient.SubmitDesignToOverride(
              request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.dataEdit.assetId, request.dataEdit.onMachineDesignName, startTime,
              endTime);
        }
        if (request.dataEdit.liftNumber.HasValue && returnResult == TTAGProcServerProcessResult.tpsprOK)
        {
          //Lift number
          returnResult = tagClient.SubmitLayerToOverride(
              request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.dataEdit.assetId, request.dataEdit.liftNumber.Value, startTime, endTime);
        }
      }

      if (returnResult == TTAGProcServerProcessResult.tpsprOK)
        return new ContractExecutionResult();

      throw CreateServiceException<EditDataExecutor>((int)returnResult);
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddTagProcessorErrorMessages(ContractExecutionStates);
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
