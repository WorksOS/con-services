using System;
using VSS.TRex.Filters;
using VSS.TRex.Types;

namespace VSS.TRex.GridFabric.Arguments
{
    /// <summary>
    /// Contains all the parameters necessary to be sent for a generic subgrids request made to the compute cluster
    /// </summary>
    [Serializable]
    public class SubGridsRequestArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public Guid SiteModelID = Guid.Empty;

        /// <summary>
        /// The request ID for the subgrid request
        /// </summary>
        public long RequestID = -1;

        /// <summary>
        /// The grid data type to extract from the processed subgrids
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// The serialised contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all subgrids that need to be requested for production data
        /// </summary>
        public byte[] ProdDataMaskBytes { get; set; }

        /// <summary>
        /// The serialised contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all subgrids that need to be requested for surveyed surface data ONLY
        /// </summary>
        public byte[] SurveyedSurfaceOnlyMaskBytes { get; set; }

        /// <summary>
        /// The name of the message topic that subgrid responses should be sent to
        /// </summary>
        public string MessageTopic { get; set; } = string.Empty;

        /// <summary>
        /// Denotes whether results of these requests should include any surveyed surfaces in the site model
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridsRequestArgument()
        {
        }
    }
}
