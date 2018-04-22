using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Execute internal business logic to handle submission of a TAG file to TRex
    /// </summary>
    public class SubmitTAGFileExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Receive a TAG file to be processed, validate TAG File Authorisation for the file, and add it to the 
        /// queue to be processed.
        /// </summary>
        /// <param name="ProjectID">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
        /// <param name="AssetID">Asset ID to be used as an override to any Asste ID that may be determined via TAG file authorization</param>
        /// <param name="TAGFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
        /// <returns></returns>
        public static SubmitTAGFileResponse Execute(long ProjectID, long AssetID,
            byte [] TAGFileContent)
        {
            // Execute TFA based business logic along with override IDs to determine final project and asset
            // identities to be used for processing the TAG file
            // ...

            // Place the validated TAG file content and processing meta data (project ID, asset ID, etc) into
            // the TAG file processing queue cache.
            // ...

            return new SubmitTAGFileResponse();
        }
    }
}
