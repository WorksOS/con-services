using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Subscription.Processor.Interfaces;

namespace VSS.Subscription.Processor
{
    public class ServiceController
    {
        private readonly IConsumer _consumer;

        public ServiceController(IConsumer consumer)
        {
            _consumer = consumer;
        }
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Start()
        {
            //todo uncomment
            Log.InfoFormat("SubscriptionProcessor has been Started");
            var cancellationTokenSource = new CancellationTokenSource();
            //todo test is this is a good method or just fire a while(true) loop

            var task = Repeat.Interval(
                            TimeSpan.FromSeconds(10),
                            _consumer.Process, cancellationTokenSource.Token);

        }

        public void Stop()
        {
            Log.InfoFormat("SubscriptionProcessor has been Stopped");
        }

        public void Error()
        {
            Log.InfoFormat("SubscriptionProcessor has thrown an error");
        }
    }


    internal static class Repeat
    {
        public static Task Interval(
                TimeSpan pollInterval,
                Action action,
                CancellationToken token)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                    () =>
                    {
                        for (; ; )
                        {
                            if (token.WaitCancellationRequested(pollInterval))
                                break;

                            action();
                        }
                    }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
                this CancellationToken token,
                TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}
