using System;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
    /// <summary>
    /// Represents an internal TAG file item to be processed into a site model. It defines the underlying filename for 
    /// the TAG file, and the content of the file as a vyte array
    /// </summary>
    [Serializable]
    public class ProcessTAGFileRequestFileItem
    {
        public string FileName { get; set; }

        public byte[] TagFileContent { get; set; }

        public bool IsJohnDoe { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileRequestFileItem()
        {
        }
    }
}
