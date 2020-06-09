using System.Collections.Generic;

namespace VSS.Productivity3D.Push.Models
{
  /// <summary>
  /// Notification class that represents the payload CWS will send on Association Updates
  /// </summary>
  public class CwsTrnUpdate
  {
    public CwsTrnUpdate()
    {
      UpdatedTrns = new List<string>();
    }

    /// <summary>
    /// Account TRN 
    /// </summary>
    public string AccountTrn { get; set; }

    /// <summary>
    /// Project TRN if applicable
    /// </summary>
    public string ProjectTrn { get; set; }

    /// <summary>
    /// Any other TRNs that this effects (e.g Device)
    /// Can be null or empty
    /// </summary>
    public List<string> UpdatedTrns { get; set; }

    /// <summary>
    /// Update Type, not used - we invalidate cache for all types
    /// </summary>
    public int UpdateType { get; set; }

  }
}
