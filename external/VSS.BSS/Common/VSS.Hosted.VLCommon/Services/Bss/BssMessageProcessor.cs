using MassTransit;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssMessageProcessor
  {
    private readonly IServiceBus _serviceBus;
    private readonly IBssReference _addBssReference;
    private readonly bool _enablePublishingToServiceBus;

    public BssMessageProcessor()
    {
    }

    public BssMessageProcessor(IServiceBus serviceBus, bool enablePublishingToServiceBus, IBssReference addBssReference)
    {
      _serviceBus = serviceBus;
      _addBssReference = addBssReference;
      _enablePublishingToServiceBus = enablePublishingToServiceBus;
    }

    public Response Process<TMessage>(TMessage message) where TMessage : BssCommon
    {
      var factory = new BssWorkflowFactory(_addBssReference);
      var runner = new WorkflowRunner(_serviceBus, _enablePublishingToServiceBus);
      
      var bssResponse = new BssResponseResultProcessor();
      var logger = new BatchLoggingResultProcessor();
      var emailer = new EmailNotificationResultProcessor();

      var resultProcessors = new IWorkflowResultProcessor[] {logger, bssResponse, emailer};

      var processor = new WorkflowProcessor(factory, runner, resultProcessors);

      processor.Process(message);

      return bssResponse.Response;
    }
  }
}