using System;
using VSS.TRex.GridFabric.Models.Arguments;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details
{
  /// <summary>
  /// Argument containing the parameters required for a Pass Count details request
  /// </summary>    
  [Serializable]
  public class PassCountDetailsArgument : BaseApplicationServiceRequestArgument
  {
    // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor, which should be moved to the base class;
    /// <summary>
    /// Pass Count details values.
    /// </summary>
    public int[] PassCountDetailValues { get; set; }
  }
}
