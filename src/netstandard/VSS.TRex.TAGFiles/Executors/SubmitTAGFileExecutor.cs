using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Execute internal business logic to handle submission of a TAG file to TRex
  /// </summary>
  ///  
  public class SubmitTAGFileExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Local static/singleton TAG file buffer queue reference to use when adding TAG files to the queue
    /// </summary>
    private static TAGFileBufferQueue queue = new TAGFileBufferQueue();


    /// <summary>
    /// Receive a TAG file to be processed, validate TAG File Authorisation for the file, and add it to the 
    /// queue to be processed.
    /// </summary>
    /// <param name="projectId">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
    /// <param name="assetId">Asset ID to be used as an override to any Asset ID that may be determined via TAG file authorization</param>
    /// <param name="tagFileName">Name of the physical tag file for archiving and logging</param>
    /// <param name="tagFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
    /// <param name="tccOrgId">Used by TFA service to match VL customer to TCC org when looking for project if multiple projects and/or machine ID not in tag file</param>
    /// <returns></returns>
    public static SubmitTAGFileResponse Execute(Guid? projectId, Guid? assetId, string tagFileName, byte[] tagFileContent, string tccOrgId)
    {
      Log.LogInformation($"#In# SubmitTAGFileResponse. Processing {tagFileName} TAG file into ProjectID:{projectId}");
      
      SubmitTAGFileResponse response = new SubmitTAGFileResponse
      {
        FileName = tagFileName,
        Success = false,
        Message = "TRex unknown result (SubmitTAGFileResponse.Execute)",
        Code = (int)TRexTagFileResultCode.TRexUnknownException
      };

      try
      {
        try
        {
          // wrap up details into obj
          TagFileDetail td = new TagFileDetail()
          {
            assetId = assetId,
            projectId = projectId,
            tagFileName = tagFileName,
            tagFileContent = tagFileContent,
            tccOrgId = tccOrgId,
            IsJohnDoe = false // default
          };

          // Validate tag file submission
          var result = TagfileValidator.ValidSubmission(td).Result;
          response.Code = result.Code;
          response.Message = result.Message;
          
          if (result.Code == (int) TRexTagFileResultCode.Valid && td.projectId != null) // If OK add to process queue
          {
            // First archive the tag file
            var config = DIContext.Obtain<IConfigurationStore>();
            var tagFileArchiving = config.GetValueBool("ENABLE_TAGFILE_ARCHIVING") ?? false;
            if (tagFileArchiving)
            {
              Log.LogInformation($"#Progress# SubmitTAGFileResponse. Archiving tag file:{tagFileName}, ProjectID:{td.projectId}");
              TagFileRepository.ArchiveTagfile(td);
            }
            // switch from nullable to not nullable
            Guid validProjectId = td.projectId ?? Guid.Empty;
            Guid validAssetId = td.assetId ?? Guid.Empty;

            Log.LogInformation($"#Progress# SubmitTAGFileResponse. Submitting tag file to TagFileBufferQueue. ProjectID:{validProjectId}, AssetID:{validAssetId}, Tagfile:{tagFileName}, JohnDoe{td.IsJohnDoe} ");
            TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(tagFileName, validProjectId, validAssetId);
            TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
            {
              InsertUTC = DateTime.Now,
              ProjectID = validProjectId,
              AssetID = validAssetId,
              FileName = tagFileName,
              Content = tagFileContent,
              IsJohnDoe = td.IsJohnDoe
            };

            if (queue.Add(tagKey, tagItem)) // Add tag file to queue
            {
              response.Success = true;
              response.Message = "";
              response.Code = (int)TRexTagFileResultCode.Valid;
            }
            else
            {
              response.Code = (int)TRexTagFileResultCode.TRexQueueSubmissionError;
              response.Success = false;
              response.Message = "SubmitTAGFileResponse. Failed to submit tag file to processing queue. Request already exists";
            }
          }
          else
          {
            response.Success = false;
          }
        }
        catch (Exception e) // catch all exceptions here
        {
          response.Message = e.Message;
          Log.LogError($"#Exception# SubmitTAGFileResponse. Exception occured processing {tagFileName} Exception:{e}");
        }
      }
      finally
      {
        Log.LogInformation($"#Out# SubmitTAGFileResponse. Processed {tagFileName} Result: {response.Success}, Message:{response.Message} Code:{response.Code}");
      }
      return response;
    }
  }
}
