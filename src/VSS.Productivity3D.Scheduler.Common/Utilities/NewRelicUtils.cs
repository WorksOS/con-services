using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
{
  public static class NewRelicUtils
  {
    private static string newRelicServiceName = "3dPmScheduler";

    public static void NotifyNewRelic(string scheduledTask, string errorLevel, DateTime startTimeUtc, double elapsedMs, Dictionary<string, object> eventAttributes = null)
    {
     var fullEventAttributes = new Dictionary<string, object>
     {
       {"scheduledTask", "DatabaseCleanupTask"},
       {"errorLevel", "Fatal"},
       {"startTimeUtc", startTimeUtc},
       {"elapsedMs", (DateTime.UtcNow - startTimeUtc).TotalMilliseconds}
     };

      if (eventAttributes != null)
      {
        fullEventAttributes.AddRange(eventAttributes);
      }

#if NET_4_7
      NewRelic.Api.Agent.NewRelic.RecordCustomEvent(newRelicServiceName, fullEventAttributes);
#endif
      Console.WriteLine($"newRelicServiceName {newRelicServiceName} eventAttributes {JsonConvert.SerializeObject(fullEventAttributes)}");
    }

    public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
    {
      if (target == null)
        throw new ArgumentNullException(nameof(target));
      if (source == null)
        throw new ArgumentNullException(nameof(source));
      foreach (var element in source)
        target.Add(element);
    }

  }
}

