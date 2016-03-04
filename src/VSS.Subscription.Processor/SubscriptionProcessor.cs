using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.Kafka.DotNetClient.Consumer;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.Messaging.Kafka.Interfaces;
using VSS.Subscription.Model.Interfaces;
using VSS.Subscription.Processor.Consumer;
using VSS.Subscription.Processor.Interfaces;

namespace VSS.Subscription.Processor
{
  public class SubscriptionProcessor : ISubscriptionProcessor
  {
    private readonly IObserver<ConsumerInstanceResponse> _observer;
    private readonly IDisposable _subscriber;
    private readonly ConsumerWrapper _consumerWrapper;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public SubscriptionProcessor(ISubscriptionService service, IConsumerConfigurator configurator)
    {
        try
        {
            _consumerWrapper = new ConsumerWrapper(configurator);
            _subscriber = _consumerWrapper.Subscribe(new SubscriptionEventObserver(service));
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
