using log4net;
using MassTransit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;

namespace VSS.Nighthawk.NHDataSvc.Consumers
{
  public class BatchedNHDataIdsEventConsumer: Consumes<IBatchedNHDataIdsEvent>.Context
  {
    protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly INHDataProcessor _nhDataProcessor;
    private readonly HttpClient _client;
    private readonly List<MediaTypeFormatter> _mediaTypeFormatters;
    //private bool batch = false;

    public BatchedNHDataIdsEventConsumer(INHDataProcessor nhData, HttpClient client, List<MediaTypeFormatter> mediaTypeFormatters)
    {
      _nhDataProcessor = nhData;
      _client = client;
      _mediaTypeFormatters = mediaTypeFormatters;
    }

    public virtual void Consume(IConsumeContext<IBatchedNHDataIdsEvent> context)
    {
      if (context == null)
      {
        Log.IfError("Received empty context");
      }
      else
      {
        try
        {
          if ((context.Message == null) || (string.IsNullOrEmpty(context.Message.URL)))
          {
            Log.IfError("Received message that did not have a URL in it");
          }
          else if (context.Message.Ids == null || context.Message.Ids.Length == 0)
          {
            Log.IfError("Received message that did not have any ids in it");
          }
          else
          {
            var json = JsonConvert.SerializeObject(context.Message.Ids);
            Log.IfDebugFormat("Requesting data from {0} for ids {1}", context.Message.URL, json);
            var result = _client.PostAsync(context.Message.URL, new StringContent(json,  System.Text.Encoding.UTF8,"application/json")).Result;
            result.EnsureSuccessStatusCode();

            var dataWrappers = result.Content.ReadAsAsync<List<NHDataWrapper>>(_mediaTypeFormatters).Result;

            if (dataWrappers != null && dataWrappers.Count > 0)
            {
              _nhDataProcessor.Process(dataWrappers);
            }
          }
        }
        catch(InvalidOperationException ioe)
        {
          if (ioe.Message.Contains("An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set"))
          {
            Log.IfErrorFormat(ioe, "Invalid URL {0} in message; placing on error queue", context.Message.URL);
            throw ioe;
          }
          else
          {
            Log.IfError("Unexpected Error Processing message from RabbitMQ", ioe);
            context.RetryLater();
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
