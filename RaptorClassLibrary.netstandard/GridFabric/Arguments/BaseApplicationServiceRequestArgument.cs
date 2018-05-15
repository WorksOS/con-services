using System;

namespace VSS.TRex.GridFabric.Arguments
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
    }
}
