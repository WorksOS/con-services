using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses
{
    [Serializable]
    public class ProcessTAGFileResponseItem
    {
        public string FileName { get; set; }

        public bool Success { get; set; } = false;

        public string Exception { get; set; } = null;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileResponseItem()
        {
        }

    }

    [Serializable]
    public class ProcessTAGFileResponse
    {
        public List<ProcessTAGFileResponseItem> Results { get; set; } = new List<ProcessTAGFileResponseItem>();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileResponse()
        {
        }
    }
}
