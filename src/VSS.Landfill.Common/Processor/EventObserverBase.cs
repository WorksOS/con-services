using System;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using org.apache.kafka.clients.consumer;
using VSS.Landfill.Common.Helpers;
using VSS.Landfill.Common.JsonConverters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Landfill.Common.Processor
{
  public abstract class EventObserverBase<T, U> : IObserver<ConsumerRecord> where U : JsonCreationConverter<T>, new()
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected string EventName { get; set; } //for logging

    public void OnCompleted()
    {
      Log.Info(string.Format("Completed consuming {0} event messages", EventName));
    }

    public void OnError(Exception error)
    {
      Log.DebugFormat("Failed consuming {1} event messages: {0} ", error.ToString(), EventName);
    }

    public void OnNext(ConsumerRecord value)
    {
      Log.DebugFormat("EventObserverBase.OnNext() for {0} events", EventName);
      try
      {
        string val = (string)value.value();

          Log.DebugFormat("Received {1}  Payload : {0} ", val, EventName);

        T evt = JsonConvert.DeserializeObject<T>(val, new U());
        bool success = ProcessEvent(evt);

        if (success)
        {
          if (Log.IsDebugEnabled)
            Log.Debug("Consumed " + evt.GetType().Name);
        }
        else
        {
          if (Log.IsWarnEnabled)
            Log.WarnFormat("Consumed a message but discarded as not relevant {0}... ", val.Truncate(30));
        }

      }
      catch (MySqlException ex)
      {
        Log.Error(string.Format("MySql Error  occured while Processing the {0} Payload", EventName), ex);
        switch (ex.Number)
        {
          case 0: //Cannot connect to server
          case 1045: //Invalid user name and/or password
            throw;
          default:
            //todo: log exception and payload here
            break;
        }
      }
      catch (Exception ex)
      {
        //deliberately suppress
        Log.Error(string.Format("Error  occured while Processing the {0} Payload", EventName), ex);
      }
    }

    protected abstract bool ProcessEvent(T evt);

  }
}
