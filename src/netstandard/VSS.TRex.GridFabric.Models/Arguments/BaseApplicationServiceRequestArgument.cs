using System;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.GridFabric.Models.Arguments
{
  /// <summary>
  ///  Forms the base request argument state that specific application service request contexts may leverage. It's roles include
  ///  containing the identifier of a TRex Application Service Node that originated the request
  /// </summary>
  [Serializable]
  public class BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The identifier of the TRex node responsible for issuing a request and to which messages containing responses
    /// should be sent on a message topic contained within the derived request. 
    /// </summary>
    public string TRexNodeID { get; set; } = string.Empty;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The set of filters to be applied to the requested subgrids
    /// </summary>
    public IFilterSet Filters { get; set; }

    /// <summary>
    /// The design to be used in cases of cut/fill subgrid requests
    /// </summary>
    public Guid CutFillDesignID { get; set; } = Guid.Empty;

    // TODO  LiftBuildSettings  :TICLiftBuildSettings;
  }
}
