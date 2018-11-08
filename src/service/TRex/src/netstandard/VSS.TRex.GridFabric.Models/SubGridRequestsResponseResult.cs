namespace VSS.TRex.GridFabric.Models
{
    /// <summary>
    /// The general response result code returned by compute cluster nodes in response to subgrids requests
    /// </summary>
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
