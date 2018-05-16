using System;
using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{   
    [Serializable]
    public class ProcessTAGFileRequestArgument
    {
        /// <summary>
        /// ID of the project to process the TAG files into
        /// </summary>
        public Guid ProjectID { get; set; } = Guid.Empty;

        /// <summary>
        /// ID of the asset to process the TAG files into
        /// </summary>
        // public long AssetID { get; set; } = -1;
        public Guid AssetID { get; set; }
    
        /// <summary>
        /// A dictionary mapping TAG file nams to the content of each file
        /// </summary>
        public List<ProcessTAGFileRequestFileItem> TAGFiles { get; set; } 

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public ProcessTAGFileRequestArgument()
        {
        }
    }
}
