using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using log4net;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Location;

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

     // private const string URL = "https://sub.domain.com/objects.json?api_key=123";
     // private const string DATA = @"{""object"":{""name"":""Name""}}";

      // Do the TFA thing here



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
        public static SubmitTAGFileResponse Execute(long projectId, Guid assetId, string tagFileName,
            byte[] tagFileContent, string tccOrgId)
        {
            // Execute TFA based business logic along with override IDs to determine final project and asset
            // identities to be used for processing the TAG file
            // ...

            Log.Info($"#In# SubmitTAGFileResponse. Processing {tagFileName} TAG file into project {projectId}, asset {assetId}");

            SubmitTAGFileResponse response = new SubmitTAGFileResponse
            {
                FileName = tagFileName,
                Success = false,
                Exception = "Unknown"
            };

            TagfileDetail td = new TagfileDetail()
                                        {
                                            assetId = assetId,
                                            projectId = projectId,
                                            tagFileName = tagFileName,
                                            tagFileContent = tagFileContent,
                                            tccOrgId = tccOrgId
                                        };

            // Validate tagfile submission
            var result = TagfileValidator.ValidSubmission(td);
            if (result == ValidationResult.Valid) // If OK add to process queue
            {
                // Archive the tagfile
                Log.Info($"Archiving tagfile {tagFileName} for project {projectId}");
                TagfileReposity.ArchiveTagfile(td); // todo implement


                Log.Info($"Pushing tagfile to TagfileBufferQueue");
                TAGFileBufferQueueKey tagKey =
                    new TAGFileBufferQueueKey(tagFileName, projectId, assetId /*projectUID, assetUID*/);

                // todo AssetID is now GUID

                TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
                {
                    InsertUTC = DateTime.Now,
                    //ProjectUID = projectUID,
                    //AssetUID = Guid.NewGuid(),
                    ProjectID = projectId,
                    AssetID = assetId,
                    FileName = tagFileName,
                    Content = tagFileContent
                };

                if (queue.Add(tagKey, tagItem))
                {
                    response.Success = true;
                    response.Exception = "";
                }
                else
                {
                    response.Success = false;
                    response.Exception = "Failed to submit tagfile to processing queue";
                }
            }

            Log.Info($"#Out# SubmitTAGFileResponse. Processed {tagFileName} Result: {response.Success}, Exception:{response.Exception}");

            return response;
        }
    }
}
