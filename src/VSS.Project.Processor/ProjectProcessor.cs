using log4net;
using System;
using System.Reflection;
using VSS.Kafka.DotNetClient.Model;
using VSS.Project.Data.Interfaces;
using VSS.Project.Processor.Consumer;
using VSS.Project.Processor.Interfaces;

namespace VSS.Project.Processor
{
  public class ProjectProcessor : IProjectProcessor
  {
    private readonly IObserver<ConsumerInstanceResponse> _observer;
    private readonly IDisposable _subscriber;
    private readonly ConsumerWrapper _consumerWrapper;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public ProjectProcessor(IProjectService service, IConsumerConfigurator configurator)
    {
        try
        {
            _consumerWrapper = new ConsumerWrapper(configurator);
            _subscriber = _consumerWrapper.Subscribe(new ProjectEventObserver(service));
        }
        catch (Exception error)
        {
            Log.Error("Error creating the consumer" + error.Message + error.StackTrace,error);
        }
    }

    public void Process()
    {
        _consumerWrapper.StartConsume();
    }

    public void Stop()
    {
        if (_subscriber != null)
            _subscriber.Dispose();
        _consumerWrapper.Dispose();
    }
  }
}
