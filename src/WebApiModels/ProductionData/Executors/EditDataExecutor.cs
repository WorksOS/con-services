using ShineOn.Rtl;
using System;
using System.Net;
using TAGProcServiceDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Velociraptor.PDSInterface.Client.TAGProcessor;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class EditDataExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public EditDataExecutor()
    {
      ProcessErrorCodes();
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;

      try
      {
        EditDataRequest request = item as EditDataRequest;
        //Note: request.dataEdit should only be null for a global undo. This is checked in request model validation
        //so the following should never happen. But just in case...
        if (request.dataEdit == null && !request.undo)
        {
          result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "No data edit to peform");
        }
        else
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
            DateTime st = request.dataEdit.startUTC;
            startTime = new TDateTime((ushort)st.Year, (ushort)st.Month, (ushort)st.Day, (ushort)st.Hour,
                (ushort)st.Minute, (ushort)st.Second, (ushort)st.Millisecond);
            DateTime et = request.dataEdit.endUTC;
            endTime = new TDateTime((ushort)et.Year, (ushort)et.Month, (ushort)et.Day, (ushort)et.Hour,
                (ushort)et.Minute, (ushort)et.Second, (ushort)et.Millisecond);
          }
          TTAGProcServerProcessResult returnResult = TTAGProcServerProcessResult.tpsprOK;
          TAGProcessorClient tagClient = tagProcessor.ProjectDataServerTAGProcessorClient();
          if (request.undo)
          {
            if (request.dataEdit == null)
            {
              returnResult = tagClient.SubmitOverrideDesignRemove(request.projectId ?? -1, -1, new TDateTime());
              if (returnResult == TTAGProcServerProcessResult.tpsprOK)
                returnResult = tagClient.SubmitOverrideLayerRemove(request.projectId ?? -1, -1, new TDateTime());
            }
            else
            {
              if (!string.IsNullOrEmpty(request.dataEdit.onMachineDesignName))
              {
                returnResult = tagClient.SubmitOverrideDesignRemove(request.projectId ?? -1, request.dataEdit.assetId,
                    startTime);
              }
              if (request.dataEdit.liftNumber.HasValue && returnResult == TTAGProcServerProcessResult.tpsprOK)
              {
                returnResult = tagClient.SubmitOverrideLayerRemove(request.projectId ?? -1, request.dataEdit.assetId,
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
                  request.projectId ?? -1, request.dataEdit.assetId, request.dataEdit.onMachineDesignName, startTime,
                  endTime);
            }
            if (request.dataEdit.liftNumber.HasValue && returnResult == TTAGProcServerProcessResult.tpsprOK)
            {
              //Lift number
              returnResult = tagClient.SubmitLayerToOverride(
                  request.projectId ?? -1, request.dataEdit.assetId, request.dataEdit.liftNumber.Value, startTime, endTime);
            }
          }
          if (returnResult == TTAGProcServerProcessResult.tpsprOK)
            result = new ContractExecutionResult();
          else
            throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStates.GetErrorNumberwithOffset((int)returnResult),
                  $"Production data edit failed: {ContractExecutionStates.FirstNameWithOffset((int) returnResult)}"));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddTagProcessorErrorMessages(ContractExecutionStates);
    }
  }
}