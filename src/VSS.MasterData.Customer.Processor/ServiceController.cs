using log4net;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VSP.MasterData.Common.Logging;
using VSS.MasterData.Customer.Processor.Interfaces;

namespace VSS.MasterData.Customer.Processor
{
  public class ServiceController
  {
    private readonly ICustomerProcessor _consumer;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ServiceController(ICustomerProcessor consumer)
    {
      _consumer = consumer;
    }

    public void Start()
    {

      try
      {
      _consumer.Process();
      }
      catch (Exception ex)
      {
        Log.IfInfo(string.Format("Failed to start Customer Processor.. \n {0} \n {1}", ex.Message, ex.StackTrace));
      }
      Log.IfInfo("Customer Processor has been Started");

      //var cancellationTokenSource = new CancellationTokenSource();
      ////todo test is this is a good method or just fire a while(true) loop

      //var task = Repeat.Interval(
      //                TimeSpan.FromSeconds(10),
      //                _consumer.Process, cancellationTokenSource.Token);
    }

    public void Stop()
    {
      _consumer.Stop();
      Log.Info("Customer Processor has been Stopped");
    }

    public void Error()
    {
      Log.Error("Customer Processor has been Stopped");
    }
  }
  //internal static class Repeat
  //{
  //    public static Task Interval(
  //            TimeSpan pollInterval,
  //            Action action,
  //            CancellationToken token)
  //    {
  //        // We don't use Observable.Interval:
  //        // If we block, the values start bunching up behind each other.
  //        return Task.Factory.StartNew(
  //                () =>
  //                {
  //                    for (; ; )
  //                    {
  //                        if (token.WaitCancellationRequested(pollInterval))
  //                            break;

  //                        action();
  //                    }
  //                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
  //    }
  //}

  //static class CancellationTokenExtensions
  //{
  //    public static bool WaitCancellationRequested(
  //            this CancellationToken token,
  //            TimeSpan timeout)
  //    {
  //        return token.WaitHandle.WaitOne(timeout);
  //    }
  //}
}
