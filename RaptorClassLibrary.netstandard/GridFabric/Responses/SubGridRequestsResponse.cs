using System;
using VSS.TRex.GridFabric.Types;

namespace VSS.TRex.GridFabric.Responses
{
    /// <summary>
    /// SubGridRequestsResponse represents the formal completion response sent back to a requestor from a 
    /// SubGridRequests request. Content includes the cluster node identity returning the response, a general response
    /// code covering the request plus additional statistical data such as the number of subgrids processed by 
    /// that cluster node from the overall pool of subgrid requested
    /// </summary>
    [Serializable]
    public class SubGridRequestsResponse
    {
        /// <summary>
        /// The general subgrids request response code returned for the request
        /// </summary>
        public SubGridRequestsResponseResult ResponseCode { get; set; } = SubGridRequestsResponseResult.Unknown;

        /// <summary>
        /// The moniker of the cluster node making the response
        /// </summary>
        public string ClusterNode { get; set; } = string.Empty;

        /// <summary>
        /// The number of subgrids in the total subgrids request processed by the responding cluster node
        /// </summary>
        public long NumSubgridsProcessed { get; set; } = -1;

        /// <summary>
        /// The total number of subgrids scanned by the processing cluster node. This should match the overall number
        /// of subgrids in the request unless ResponseCode indicates a failure.
        /// </summary>
        public long NumSubgridsExamined { get; set; } = -1;

        /// <summary>
        /// The number of subgrids containing production data in the total subgrids request processed by the responding cluster node
        /// </summary>
        public long NumProdDataSubGridsProcessed { get; set; } = -1;

        /// <summary>
        /// The total number of subgrids containing production data scanned by the processing cluster node. This should match the overall number
        /// of production data subgrids in the request unless ResponseCode indicates a failure.
        /// </summary>
        public long NumProdDataSubGridsExamined { get; set; } = -1;

        /// <summary>
        /// The number of subgrids containing survyed surfaces data in the total subgrids request processed by the responding cluster node
        /// </summary>
        public long NumSurveyedSurfaceSubGridsProcessed { get; set; } = -1;

        /// <summary>
        /// The total number of subgrids containing surveyed surface data scanned by the processing cluster node. This should match the overall number
        /// of surveyed surface subgrids in the request unless ResponseCode indicates a failure.
        /// </summary>
        public long NumSurveyedSurfaceSubGridsExamined { get; set; } = -1;
    }
}
