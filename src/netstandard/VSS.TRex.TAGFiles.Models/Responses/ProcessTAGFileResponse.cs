using System;
using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
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
