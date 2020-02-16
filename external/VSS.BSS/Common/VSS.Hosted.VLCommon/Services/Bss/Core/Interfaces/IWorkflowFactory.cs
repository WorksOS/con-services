namespace VSS.Hosted.VLCommon.Bss
{
  public interface IWorkflowFactory
  {
    IWorkflow Create<TMessage>(TMessage message);
  }
}