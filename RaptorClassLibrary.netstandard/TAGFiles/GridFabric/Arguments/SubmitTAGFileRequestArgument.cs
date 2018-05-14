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
        public Guid ProjectID { get; set; } = Guid.Empty;

        /// <summary>
        /// Overridden ID of the asset to process the TAG files into
        /// </summary>
        //public long AssetID { get; set; } = -1;
        public Guid AssetID { get; set; }

        /// <summary>
        /// Name of physical tagfile
        /// </summary>
        public string TAGFileName { get; set; } = string.Empty;

        /// <summary>
        /// The content of the TAG file being submitted
        /// </summary>
        public byte[] TagFileContent { get; set; }

        /// <summary>
        /// Helps TFA service determine correct project
        /// </summary>
        public string TCCOrgID { get; set; } = string.Empty;

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public SubmitTAGFileRequestArgument()
        {
        }
    }
}
