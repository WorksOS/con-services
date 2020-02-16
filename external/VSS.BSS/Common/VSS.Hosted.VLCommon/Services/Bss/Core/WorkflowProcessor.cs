namespace VSS.Hosted.VLCommon.Bss
{
  public class WorkflowProcessor
  {
    private readonly IWorkflowFactory _factory;
    private readonly IWorkflowRunner _runner;
    private readonly IWorkflowResultProcessor[] _resultProcessors;

    public WorkflowProcessor(IWorkflowFactory factory, IWorkflowRunner runner,
      IWorkflowResultProcessor[] resultProcessors)
    {
      _factory = factory;
      _runner = runner;
      _resultProcessors = resultProcessors;
    }

    public virtual void Process<TMessage>(TMessage message)
    {
      var workflow = _factory.Create(message);
      var result = _runner.Run(workflow);
      foreach (var resultProcessor in _resultProcessors)
      {
        resultProcessor.Process(message, result);
      }
    }
  }
}
