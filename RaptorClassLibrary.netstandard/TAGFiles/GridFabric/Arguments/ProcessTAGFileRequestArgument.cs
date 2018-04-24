using System;
using System.Collections.Generic;

namespace VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments
{   
    [Serializable]
    public class ProcessTAGFileRequestArgument
    {
        /// <summary>
        /// ID of the project to process the TAG files into
        /// </summary>
        public long ProjectID { get; set; } = -1;
        public Guid ProjectUID { get; set; }

        /// <summary>
        /// ID of the asset to process the TAG files into
        /// </summary>
        public long AssetID { get; set; } = -1;
        public Guid AssetUID { get; set; }
    
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
