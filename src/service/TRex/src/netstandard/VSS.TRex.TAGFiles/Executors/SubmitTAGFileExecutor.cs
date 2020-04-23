using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.TAGFiles.Classes;
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

    private static readonly bool TagFileArchiving = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_TAGFILE_ARCHIVING", Consts.ENABLE_TAGFILE_ARCHIVING);

    /// <summary>
    /// Local static/singleton TAG file buffer queue reference to use when adding TAG files to the queue
    /// </summary>
    private readonly ITAGFileBufferQueue _queue = DIContext.Obtain<ITAGFileBufferQueue>();

    private bool OutputInformationalRequestLogging = true;

    /// <summary>
    /// Receive a TAG file to be processed, validate TAG File Authorization for the file, and add it to the 
    /// queue to be processed.
    /// </summary>
    /// <param name="projectId">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
    /// <param name="assetId">Asset ID to be used as an override to any Asset ID that may be determined via TAG file authorization</param>
    /// <param name="tagFileName">Name of the physical tag file for archiving and logging</param>
    /// <param name="tagFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
    /// <param name="tccOrgId">Used by TFA service to match VL customer to TCC org when looking for project if multiple projects and/or machine ID not in tag file</param>
    /// <returns></returns>
    public async Task<SubmitTAGFileResponse> ExecuteAsync(Guid? projectId, Guid? assetId, string tagFileName, byte[] tagFileContent, string tccOrgId)
    {
      if (OutputInformationalRequestLogging)
        Log.LogInformation($"#In# SubmitTAGFileResponse. Processing {tagFileName} TAG file into ProjectUID:{projectId}, asset:{assetId}");
      
      var response = new SubmitTAGFileResponse
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
          var td = new TagFileDetail
          {
            assetId = assetId,
            projectId = projectId,
            tagFileName = tagFileName,
            tagFileContent = tagFileContent,
            tccOrgId = tccOrgId,
            IsJohnDoe = false // default
          };

          // Validate tag file submission
          var result = await TagfileValidator.ValidSubmission(td);
          response.Code = result.Code;
          response.Message = result.Message;
          
          if (result.Code == (int) TRexTagFileResultCode.Valid && td.projectId != null) // If OK add to process queue
          {
            // First archive the tag file
            if (TagFileArchiving)
            {
              Log.LogInformation($"#Progress# SubmitTAGFileResponse. Archiving tag file:{tagFileName}, ProjectUID:{td.projectId}");
              TagFileRepository.ArchiveTagfile(td);
            }
            // switch from nullable to not nullable
            var validProjectId = td.projectId ?? Guid.Empty;
            var validAssetId = td.assetId ?? Guid.Empty;

            if (OutputInformationalRequestLogging)
              Log.LogInformation($"#Progress# SubmitTAGFileResponse. Submitting tag file to TagFileBufferQueue. ProjectUID:{validProjectId}, AssetUID:{validAssetId}, Tagfile:{tagFileName}, JohnDoe:{td.IsJohnDoe} ");

            var tagKey = new TAGFileBufferQueueKey(tagFileName, validProjectId, validAssetId);
            var tagItem = new TAGFileBufferQueueItem
            {
              InsertUTC = DateTime.UtcNow,
              ProjectID = validProjectId,
              AssetID = validAssetId,
              FileName = tagFileName,
              Content = tagFileContent,
              IsJohnDoe = td.IsJohnDoe
            };

            if (_queue == null)
            {
              response.Success = false;
              response.Message = "SubmitTAGFileResponse. Processing queue not available";
              response.Code = (int)TRexTagFileResultCode.TRexTagFileSubmissionQueueNotAvailable;

              return response;
            }

            if (_queue.Add(tagKey, tagItem)) // Add tag file to queue
            {
              response.Success = true;
              response.Message = "";
              response.Code = (int)TRexTagFileResultCode.Valid;

              // Commented out top reduce logging
              // Log.LogInformation($"Added TAG file {tagKey.FileName} representing asset {tagKey.AssetUID} within project {tagKey.ProjectUID} into the buffer queue");
            }
            else
            {
              response.Success = false;
              response.Message = "SubmitTAGFileResponse. Failed to submit tag file to processing queue. Request already exists";
              response.Code = (int)TRexTagFileResultCode.TRexQueueSubmissionError;

              Log.LogWarning(response.Message);
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
          Log.LogError(e, $"#Exception# SubmitTAGFileResponse. Exception occured processing {tagFileName} Exception:");
        }
      }
      finally
      {
        if (OutputInformationalRequestLogging)
          Log.LogInformation($"#Out# SubmitTAGFileResponse. Processed {tagFileName} Result: {response.Success}, Message:{response.Message} Code:{response.Code}");
      }
      return response;
    }
  }
}
