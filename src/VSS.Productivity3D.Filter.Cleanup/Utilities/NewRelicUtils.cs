using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Filter.Cleanup.Utilities
{
  public static class NewRelicUtils
  {
    private static string newRelicServiceName = "3dPmFilterCleanupScheduler";

    public static void NotifyNewRelic(string scheduledTask, string messageLevel, DateTime startTimeUtc,
      ILogger log, Dictionary<string, object> eventAttributes = null)
    {
      var fullEventAttributes = new Dictionary<string, object>
      {
        {"scheduledTask", scheduledTask},
        {"messageLevel", messageLevel},
        {"startTimeUtc", startTimeUtc},
        {"elapsedMs", (DateTime.UtcNow - startTimeUtc).TotalMilliseconds}
      };

      if (eventAttributes != null)
      {
        fullEventAttributes.AddRange(eventAttributes);
      }

      NewRelic.Api.Agent.NewRelic.RecordCustomEvent(newRelicServiceName, fullEventAttributes);

      var logMessage =
        $"NewRelicServiceName: {newRelicServiceName} eventAttributes: {JsonConvert.SerializeObject(fullEventAttributes)}";
      if (string.Compare(messageLevel, "Error", StringComparison.OrdinalIgnoreCase) == 0)
        log.LogError(logMessage);
      else
        log.LogInformation(logMessage);
    }

    public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
    {
      if (target != null && source != null)
      {
        foreach (var element in source)
          target.Add(element);
      }
    }
  }
}

