using System;

namespace VSS.TRex.GridFabric.Types
{
    /// <summary>
    /// The general response result code returned by compute cluster nodes in response to subgrids requests
    /// </summary>
    [Serializable]
    public enum SubGridRequestsResponseResult
    {
        OK,
        Failure,
        NoIgniteGroupProjection,
        NoIgniteMessagingProjection,
        NotImplemented,
        Unknown
    }
}
