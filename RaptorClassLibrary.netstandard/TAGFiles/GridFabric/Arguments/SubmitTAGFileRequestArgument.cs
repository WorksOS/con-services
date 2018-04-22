using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
    [Serializable]
    public class SubmitTAGFileRequestArgument
    {
        /// <summary>
        /// Overridden ID of the project to process the TAG files into
        /// </summary>
        public long ProjectID { get; set; } = -1;

        /// <summary>
        /// Overridden ID of the asset to process the TAG files into
        /// </summary>
        public long AssetID { get; set; } = -1;

        public string TAGFileName { get; set; } = string.Empty;

        /// <summary>
        /// The content of the TAG file being submitted
        /// </summary>
        public byte[] TagFileContent { get; set; }

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public SubmitTAGFileRequestArgument()
        {
        }
    }
}
