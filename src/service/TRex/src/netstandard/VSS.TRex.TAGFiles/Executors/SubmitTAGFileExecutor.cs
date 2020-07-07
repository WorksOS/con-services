using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
  public class SubmitTAGFileExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubmitTAGFileExecutor>();

    private static readonly bool _tagFileArchiving = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_TAGFILE_ARCHIVING", Consts.ENABLE_TAGFILE_ARCHIVING);

    /// <summary>
    /// Local static/singleton TAG file buffer queue reference to use when adding TAG files to the queue
    /// </summary>
    private readonly ITAGFileBufferQueue _queue = DIContext.Obtain<Func<ITAGFileBufferQueue>>()();

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
    /// <param name="treatAsJohnDoe">The TAG file will be processed as if it were a john doe machine is projectId is also specified</param>
    /// <param name="tagFileSubmissionFlags">A flag set controlling how certain aspects of managing a submitted TAG file should be managed</param>
    public async Task<SubmitTAGFileResponse> ExecuteAsync(Guid? projectId, Guid? assetId, string tagFileName, byte[] tagFileContent, 
      string tccOrgId, bool treatAsJohnDoe, TAGFileSubmissionFlags tagFileSubmissionFlags)
    {
      if (OutputInformationalRequestLogging)
        _log.LogInformation($"#In# SubmitTAGFileResponse. Processing {tagFileName} TAG file into ProjectUID:{projectId}, asset:{assetId}");
      
      var response = new SubmitTAGFileResponse
      {
        FileName = tagFileName,
        Success = false,
        Message = "TRex unknown result (SubmitTAGFileResponse.Execute)",
        Code = (int)TRexTagFileResultCode.TRexUnknownException,
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
            IsJohnDoe = treatAsJohnDoe
          };

          // Validate tag file submission
          var result = await TagfileValidator.ValidSubmission(td);
          response.Code = result.Code;
          response.Message = result.Message;
          
          if (result.Code == (int) TRexTagFileResultCode.Valid && td.projectId != null) // If OK add to process queue
          {
            // First archive the tag file
            if (_tagFileArchiving && tagFileSubmissionFlags.HasFlag(TAGFileSubmissionFlags.AddToArchive))
            {
              _log.LogInformation($"#Progress# SubmitTAGFileResponse. Archiving tag file:{tagFileName}, ProjectUID:{td.projectId}");
              if (! await TagFileRepository.ArchiveTagfileS3(td))
              {
                _log.LogError($"SubmitTAGFileResponse. Failed to archive tag file. Returning TRexQueueSubmissionError error. ProjectUID:{td.projectId}, AssetUID:{td.assetId}, Tagfile:{tagFileName}");
                throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"SubmitTAGFileResponse. Failed to archive tag file {tagFileName} to S3"));
              }
            }

            // switch from nullable to not nullable
            var validProjectId = td.projectId ?? Guid.Empty;
            var validAssetId = td.assetId ?? Guid.Empty;

            if (OutputInformationalRequestLogging)
              _log.LogInformation($"#Progress# SubmitTAGFileResponse. Submitting tag file to TagFileBufferQueue. ProjectUID:{validProjectId}, AssetUID:{validAssetId}, Tagfile:{tagFileName}, JohnDoe:{td.IsJohnDoe} ");

            var tagKey = new TAGFileBufferQueueKey(tagFileName, validProjectId, validAssetId);
            var tagItem = new TAGFileBufferQueueItem
            {
              InsertUTC = DateTime.UtcNow,
              ProjectID = validProjectId,
              AssetID = validAssetId,
              FileName = tagFileName,
              Content = tagFileContent,
              IsJohnDoe = td.IsJohnDoe,
              SubmissionFlags = tagFileSubmissionFlags
            };

            if (_queue == null)
            {
              throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "SubmitTAGFileResponse. Processing queue not available"));
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

              _log.LogWarning(response.Message);
            }
          }
          else
          {
            response.Success = false;
          }
        }
        catch (Exception e) // catch all exceptions here
        {
          _log.LogError(e, $"#Exception# SubmitTAGFileResponse. Exception occured processing {tagFileName} Exception:");
          throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"SubmitTAGFileResponse. Exception {e.Message}"));
        }
      }
      finally
      {
        if (OutputInformationalRequestLogging)
          _log.LogInformation($"#Out# SubmitTAGFileResponse. Processed {tagFileName} Result: {response.Success}, Message:{response.Message} Code:{response.Code}");
      }
      return response;
    }
  }
}
