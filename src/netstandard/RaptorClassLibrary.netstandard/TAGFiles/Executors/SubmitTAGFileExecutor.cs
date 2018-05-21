using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Execute internal business logic to handle submission of a TAG file to TRex
    /// </summary>
    ///  
    public class SubmitTAGFileExecutor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

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
        /// <param name="projectId">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
        /// <param name="assetId">Asset ID to be used as an override to any Asset ID that may be determined via TAG file authorization</param>
        /// <param name="tagFileName">Name of the physical tagfile for archiving and logging</param>
        /// <param name="tagFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
        /// <param name="tccOrgId">Used by TFA service to match VL customer to TCC org when looking for project if multiple projects and/or machine ID not in tag file</param>
        /// <returns></returns>
        public static SubmitTAGFileResponse Execute(Guid projectId, Guid assetId, string tagFileName,
            byte[] tagFileContent, string tccOrgId)
        {
            // Execute TFA based business logic along with override IDs to determine final project and asset
            // identities to be used for processing the TAG file
            // ...

            Log.LogInformation($"#In# SubmitTAGFileResponse. Processing {tagFileName} TAG file into project {projectId}, asset {assetId}");
            SubmitTAGFileResponse response = new SubmitTAGFileResponse
                                             {
                                                     FileName = tagFileName,
                                                     Success = false,
                                                     Exception = "Unknown"
                                             };
            try
            {
                try
                {
                    // wrap up details into obj
                    TagfileDetail td = new TagfileDetail()
                                       {
                                               assetId = assetId,
                                               projectId = projectId,
                                               tagFileName = tagFileName,
                                               tagFileContent = tagFileContent,
                                               tccOrgId = tccOrgId,
                                               IsJohnDoe = false // default
                                       };

                    // Validate tagfile submission
                    var result = TagfileValidator.ValidSubmission(td);

                    if (result == ValidationResult.Valid) // If OK add to process queue
                    {
                        // First archive the tagfile
                        Log.LogInformation($"Archiving tagfile {tagFileName} for project {projectId}");
                        TagfileReposity.ArchiveTagfile(td); // todo implement

                        Log.LogInformation($"Pushing tagfile to TagfileBufferQueue");
                        TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(tagFileName, projectId, assetId);
                        TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
                                                         {
                                                                 InsertUTC = DateTime.Now,
                                                                 ProjectID = projectId,
                                                                 AssetID = assetId,
                                                                 FileName = tagFileName,
                                                                 Content = tagFileContent,
                                                                 IsJohnDoe = td.IsJohnDoe
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
                    else
                    {
                        // Todo At some point a notification needs to be implemented e.g. 'api/v2/notification/tagfileprocessingerror';
                        response.Success = false;
                        response.Exception = Enum.GetName(typeof(ValidationResult),result); // return reason for failure
                    }
                }
                catch (Exception e) // catch all exceptions here
                {
                    Log.LogError($"#Exception# SubmitTAGFileResponse. Exception occured processing {tagFileName} Exception: {e}");
                }
            }
            finally
            {
                Log.LogInformation($"#Out# SubmitTAGFileResponse. Processed {tagFileName} Result: {response.Success}, Exception:{response.Exception}");
            }
            return response;
        }
    }
}
