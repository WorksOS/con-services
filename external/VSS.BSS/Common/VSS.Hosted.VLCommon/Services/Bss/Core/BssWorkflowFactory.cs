using System;
using System.Reflection;
using VSS.Hosted.VLCommon.Events;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssWorkflowFactory : IWorkflowFactory
  {
    private readonly IBssReference _addBssReference;

    public BssWorkflowFactory(IBssReference addBssReference)
    {
      _addBssReference = addBssReference;
    }

    public IWorkflow Create<TMessage>(TMessage message)
    {
      object action = message.PropertyValueByName("Action");

      if(!BssMessageAction.IsValidForMessage(action, message))
      {
        return new InvalidActionWorkflow(action, typeof(TMessage));
      }

      Type type = typeof(TMessage);
      Assembly assembly = type.Assembly;

      string workflowName = string.Format("{0}{1}Workflow", type.Name, action);
      string workflowToInitializeByAssembly = string.Format("{0}.{1}", type.Namespace, workflowName);
      string workflowToInitializeByType = string.Format("{0}.{1}", GetType().Namespace, workflowName);

      Type workflowType = assembly.GetType(workflowToInitializeByAssembly) ?? Type.GetType(workflowToInitializeByType);

      try
      {
        var inputs = new Inputs();
        inputs.Add<TMessage>(message);
        inputs.Add<EventMessageSequence>(new EventMessageSequence());
        inputs.Add<IBssReference>(_addBssReference);
        return (IWorkflow)Activator.CreateInstance(workflowType, inputs);
      }
      catch (InvalidCastException ex)
      {
        string error = string.Format(CoreConstants.WORKFLOW_NOT_IWORKFLOW, workflowType.FullName, typeof(IWorkflow));
        throw new InvalidCastException(error, ex);
      }
      catch(Exception ex)
      {
        string errorMessage = string.Format(CoreConstants.WORKFLOW_CANNOT_BE_INITIALIZED, GetType().FullName, workflowToInitializeByAssembly, workflowToInitializeByType);
        throw new InvalidOperationException(errorMessage, ex);
      }
    }
  }
}
