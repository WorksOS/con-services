namespace VSS.Hosted.VLCommon.Bss
{
  public interface IWorkflowRunner
  {
    Inputs Inputs { get; }
    WorkflowResult Run(IWorkflow workflow);
  }
}