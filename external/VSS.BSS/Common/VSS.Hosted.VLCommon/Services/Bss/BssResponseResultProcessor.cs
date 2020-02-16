using System;
using System.Linq;
using log4net;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssResponseResultProcessor : IWorkflowResultProcessor
  {
    public Response Response { get; set; }

    public void Process<TMessage>(TMessage sourceMessage, WorkflowResult workflowResult)
    {
      // Return null if Exception or Error (not BssError) encountered 
      if (workflowResult.ActivityResults.Any(x => x.GetType() == typeof(ExceptionResult)))
        return;

      if (workflowResult.ActivityResults.Any(x => x.GetType() == typeof(ErrorResult)))
        return;
      
      Response = new Response();
      Response.Success = workflowResult.Success.ToString().ToUpper();
      Response.ProcessedUTC = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", DateTime.UtcNow);
      MapMessageToResponse(sourceMessage);

      var bssError = workflowResult.ActivityResults.FirstOrDefault(x => x.GetType() == typeof(BssErrorResult)) as BssErrorResult;
      if (bssError != null)
      {
        Response.ErrorCode = bssError.FailureCode.ToString();
        Response.ErrorDescription = bssError.Summary;
      }
    }

    private void MapMessageToResponse<TMessage>(TMessage sourceMessage)
    {
      var ah = sourceMessage as AccountHierarchy;
      if (ah != null)
      {
        Response.TargetStack = ah.TargetStack;
        Response.ControlNumber = ah.ControlNumber;
        Response.SequenceNumber = ah.SequenceNumber;
        Response.EndPointName = Response.EndpointEnum.AccountHierarchy;
        return;
      }

      var ib = sourceMessage as InstallBase;
      if (ib != null)
      {
        Response.TargetStack = ib.TargetStack;
        Response.ControlNumber = ib.ControlNumber;
        Response.SequenceNumber = ib.SequenceNumber;
        Response.EndPointName = Response.EndpointEnum.InstallBase;
        return;
      }

      var dr = sourceMessage as DeviceReplacement;
      if (dr != null)
      {
        Response.TargetStack = dr.TargetStack;
        Response.ControlNumber = dr.ControlNumber;
        Response.SequenceNumber = dr.SequenceNumber;
        Response.EndPointName = Response.EndpointEnum.DeviceReplacement;
        return;
      }

      var sp = sourceMessage as ServicePlan;
      if (sp != null)
      {
        Response.TargetStack = sp.TargetStack;
        Response.ControlNumber = sp.ControlNumber;
        Response.SequenceNumber = sp.SequenceNumber;
        Response.EndPointName = Response.EndpointEnum.ServicePlan;
        return;
      }

      var dreg = sourceMessage as DeviceRegistration;
      if (dreg != null)
      {
        Response.TargetStack = dreg.TargetStack;
        Response.ControlNumber = dreg.ControlNumber;
        Response.SequenceNumber = dreg.SequenceNumber;
        Response.EndPointName = Response.EndpointEnum.DeviceRegistration;
        return;
      }
      throw new InvalidOperationException("Unhandled BSS message type in ResponseProcessor.");
    }
  }

}
