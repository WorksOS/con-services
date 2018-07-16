using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
    [Serializable]
    public class SubmitTAGFileResponse
    {
        public string FileName { get; set; }

        public bool Success { get; set; }

        public string Exception { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubmitTAGFileResponse()
        {
        }
    }
}
