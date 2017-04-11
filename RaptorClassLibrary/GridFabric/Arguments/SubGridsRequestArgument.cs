using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Arguments
{
    /// <summary>
    /// Contains all the parameters necessary to be sent for a generic subgrids request made to the compute cluster
    /// </summary>
    [Serializable]
    public class SubGridsRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public long SiteModelID = -1;

        /// <summary>
        /// The request ID for the subgrid request
        /// </summary>
        public long RequestID = -1;

        /// <summary>
        /// The grid data type to extract from the processed subgrids
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// The serialised contents of the SubGridTreeBitMask that notes the address of all subgrids that need to be requested
        /// </summary>
        public MemoryStream MaskStream { get; set; } = null;

        /// <summary>
        /// The name of the message topic that subgrid responses should be sent to
        /// </summary>
        public string MessageTopic { get; set; } = String.Empty;

        public SubGridsRequestArgument(long siteModelID, long requestID, GridDataType gridDataType, MemoryStream maskStream, string messageTopic)
        {
            SiteModelID = siteModelID;
            RequestID = requestID;
            GridDataType = gridDataType;
            MaskStream = maskStream;
            MessageTopic = messageTopic;
        }
    }
}
