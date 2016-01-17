using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using log4net;
using VSS.VisionLink.Landfill.Common.Models;
using VSS.VisionLink.Landfill.MDM.Interfaces;

namespace VSS.VisionLink.Landfill.MDM
{
  /// <summary>
  ///   Builds pipeline of rules to be executed against inbound events
  /// </summary>
  public class RulePipelineExecutor<T>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly List<IMDMRule<T>> rules = new List<IMDMRule<T>>();

    /// <summary>
    ///   Gets the registered rules.
    /// </summary>
    /// <value>
    ///   The registered rules.
    /// </value>
    public ReadOnlyCollection<IMDMRule<T>> RegisteredRules
    {
      get { return rules.AsReadOnly(); }
    }

    public void RegisterRule(IMDMRule<T> rule)
    {
      rules.Add(rule);
    }

    /// <summary>
    ///   Cleans all rules.
    /// </summary>
    public void CleanAllRules()
    {
      rules.Clear();
    }

    public T ExecuteRules(T queuedEvent)
    {
      return ExecuteAllRulesForEvent(queuedEvent);
    }

    private T ExecuteAllRulesForEvent(T queuedEvent)
    {
      var result = queuedEvent;
      foreach (var rule in rules)
      {
        result = rule.ExecuteRule(result);
        Log.DebugFormat("LandfillDataFeed: executed rule {0} with result {1}", rule.GetType(), result.ToSafeString());
        if (result == null) return default(T);
      }
      return result;
    }
  }
}