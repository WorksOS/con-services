using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Execute internal business logic to handle submission of a TAG file to TRex
    /// </summary>
    ///  
    public class SubmitTAGFileExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Local static/singleton TAG file buffer queue reference to use when adding TAG files to the queue
        /// </summary>
        private static TAGFileBufferQueue queue = new TAGFileBufferQueue();

        private static bool ValidSubmission()
        {
            // Do the TFA thing here

            // Check file already processing??? 
            // check count of submitted tags is not to high else pause 1 sec
            // xml file ???


            return true;
        }

        /// <summary>
        /// Receive a TAG file to be processed, validate TAG File Authorisation for the file, and add it to the 
        /// queue to be processed.
        /// </summary>
        /// <param name="ProjectID">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
        /// <param name="AssetID">Asset ID to be used as an override to any Asset ID that may be determined via TAG file authorization</param>
        /// <param name="TAGFileName">Name of the physical tagfile for archiving and logging</param>
        /// <param name="TAGFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
        /// <param name="TCCOrgID">Used by TFA service to match VL customer to TCC org when looking for project if multiple projects and/or machine ID not in tag file</param>
        /// 
        /// <returns></returns>
        public static SubmitTAGFileResponse Execute(long ProjectID, long AssetID, string TAGFileName,
            byte[] TAGFileContent, string TCCOrgID)
        {
            // Execute TFA based business logic along with override IDs to determine final project and asset
            // identities to be used for processing the TAG file
            // ...

            Log.Info($"#In# SubmitTAGFileResponse. Processing {TAGFileName} TAG file into project {ProjectID}, asset {AssetID}");

            SubmitTAGFileResponse response = new SubmitTAGFileResponse
            {
                FileName = TAGFileName,
                Success = false,
                Exception = "Unknown"
            };

            // Place the validated TAG file content and processing meta data (project ID, asset ID, etc) into
            // the TAG file processing queue cache.
            // ...

            if (ValidSubmission())
            {

                //Guid projectUID = Guid.NewGuid(); // todo convert to use GUID
                //Guid assetUID = Guid.NewGuid(); // todo convert to use GUID
                TAGFileBufferQueueKey tagKey =
                    new TAGFileBufferQueueKey(TAGFileName, ProjectID, AssetID /*projectUID, assetUID*/);

                // todo AssetID is now GUID

                TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
                {
                    InsertUTC = DateTime.Now,
                    //ProjectUID = projectUID,
                    //AssetUID = Guid.NewGuid(),
                    ProjectID = ProjectID,
                    AssetID = AssetID,
                    FileName = TAGFileName,
                    Content = TAGFileContent
                };

                if (queue.Add(tagKey, tagItem))
                {
                    response.Success = true;
                    response.Exception = "";
                }
                else
                {
                    response.Success = false;
                    response.Exception = "Failed to submit to tagfile processing queue";
                }
            }

            Log.Info($"#Out# SubmitTAGFileResponse. Processed {TAGFileName} Result. Success:{response.Success}, Exception:{response.Exception}");

            return response;
        }
    }
}
