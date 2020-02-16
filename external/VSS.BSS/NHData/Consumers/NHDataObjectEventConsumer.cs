using System.Globalization;
using log4net;
using MassTransit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;
using System.Configuration;

namespace VSS.Nighthawk.NHDataSvc.Consumers
{
  public class NHDataObjectEventConsumer : Consumes<INewNhDataObjectEvent>.Context
  {
    protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly TimeSpan futureTimeThreshold = GetFutureTimeThreshold();
    private readonly INHDataProcessor _nhDataProcessor;

    private static TimeSpan GetFutureTimeThreshold()
    {
      TimeSpan futureTime;
      if (TimeSpan.TryParse(ConfigurationManager.AppSettings["FutureTimeThreshold"], out futureTime))
        return futureTime;
      else
        return TimeSpan.FromMinutes(5);  // default 5 minutes
    }

    public NHDataObjectEventConsumer(INHDataProcessor nhData)
    {
      _nhDataProcessor = nhData;
    }

    public virtual void Consume(IConsumeContext<INewNhDataObjectEvent> context)
    {
      if (context == null)
      {
        Log.IfError("Received empty context");
      }
      else
      {
        try
        {
          DateTime futureTime = DateTime.UtcNow.Add(futureTimeThreshold);
          if ((context.Message == null) || (context.Message.Message == null))
          {
            Log.IfError("Received message that did not have an INHDataObject in it");
          }
          else
          {
            if (context.Message.Message.EventUTC <= futureTime)
            {
              Log.IfInfoFormat("DateTime.UtcNow={0}. Processing message {1}",
                futureTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture),
                JsonConvert.SerializeObject(context.Message.Message));
              _nhDataProcessor.Process(new List<NHDataWrapper>
              {
                new NHDataWrapper {Data = context.Message.Message}
              });
            }
            else // context.Message.Message.EventUTC > now
            {
              Log.IfWarnFormat("Allowed Timestamp={0}. Dropping future message {1}",
                futureTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture),
                JsonConvert.SerializeObject(context.Message.Message));
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError("Unexpected Error Processing message from RabbitMQ", e);
          context.RetryLater();
        }
      }
    }
  }
}