namespace VSS.Hosted.VLCommon.Bss
{
  public interface IWorkflowResultProcessor
  {
    void Process<TMessage>(TMessage sourceMessage, WorkflowResult workflowResult);
  }
}