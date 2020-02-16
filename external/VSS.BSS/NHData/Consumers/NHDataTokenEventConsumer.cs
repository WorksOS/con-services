using log4net;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;



namespace VSS.Nighthawk.NHDataSvc.Consumers
{
  public class NHDataTokenEventConsumer : Consumes<INewNhDataTokenEvent>.Context
  {
    protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly INHDataProcessor _nhDataProcessor;
    private readonly HttpClient _client;
    private readonly List<MediaTypeFormatter> _mediaTypeFormatters;
    //private bool batch = false;

    public NHDataTokenEventConsumer(INHDataProcessor nhData, HttpClient client, List<MediaTypeFormatter> mediaTypeFormatters)
    {
      _nhDataProcessor = nhData;
      _client = client;
      _mediaTypeFormatters = mediaTypeFormatters;
    }

    public virtual void Consume(IConsumeContext<INewNhDataTokenEvent> context)
    {
      if (context == null)
      {
        Log.IfError("Received empty context");
      }
      else
      {
        try
        {
          DateTime now = DateTime.UtcNow;
          if ((context.Message == null) || (string.IsNullOrEmpty(context.Message.NHDataObjectUrl)))
          {
            Log.IfError("Received message that did not have an id in it");
          }
          else
          {
            var result = _client.GetAsync(string.Format("{0}/{1}", context.Message.NHDataObjectUrl, context.Message.Id)).Result;
            result.EnsureSuccessStatusCode();

            var dataWrappers = result.Content.ReadAsAsync<List<NHDataWrapper>>(_mediaTypeFormatters).Result;

            if (dataWrappers != null && dataWrappers.Count > 0)
            {
              _nhDataProcessor.Process(dataWrappers);
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
