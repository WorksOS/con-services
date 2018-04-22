using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses
{
    [Serializable]
    public class ProcessTAGFileResponseItem
    {
        public string FileName { get; set; }

        public bool Success { get; set; }

        public string Exception { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileResponseItem()
        {
        }
    }
}
