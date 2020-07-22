using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileGateway.Common.Executors
{
    public class TagFileProcessExecutor : RequestExecutorContainer
    {
        public const string CONNECTION_ERROR_FOLDER = ".Backend Connection Error";
        public const string INVALID_TAG_FILE_FOLDER = ".Invalid Tag File Name";
        public bool ArchiveOnInternalError = false;

        protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
        {
            if (!(item is CompactionTagFileRequest request))
            {
                Logger.LogWarning($"Invalid Request passed in. Expected {typeof(CompactionTagFileRequest).Name} but got {(item == null ? "null" : item.GetType().Name)}");
                return ContractExecutionResult.ErrorResult("Invalid Request");
            }

            request.Validate();

            Logger.LogInformation($"Received Tag File with filename: {request.FileName}. TCC Org: {request.OrgId}. Data Length: {request.Data.Length}");

            var result = ContractExecutionResult.ErrorResult("Not processed");
            var internalProcessingError = false;
            try
            {
              result = await TagFileForwarder.SendTagFileDirect(request);
            }
            catch (Exception e)
            {
              Logger.LogError(e, $"Failed to connect to TRex. Tag file {request.FileName}");
              internalProcessingError = true;
            }

            internalProcessingError = IsInternalError(internalProcessingError, result.Code);
      
            // If we failed to connect to trex (or other retry-able error),
            //     we want to either put  it separate folder or not delete from SQS que
            // If the tag file was accepted, and not processed for a real reason (e.g no project found at seed position)
            //   then we can to archive it, as it was successfully processed with no change to the datamodel
            await using (var data = new MemoryStream(request.Data))
            {
                Logger.LogInformation($"Uploading Tag File {request.FileName}");
                var path = GetS3Key(request.FileName);

                if (internalProcessingError) 
                    path = $"{CONNECTION_ERROR_FOLDER}/{path}";

                if (!internalProcessingError || ArchiveOnInternalError)
                {
                  TransferProxyFactory.NewProxy(TransferProxyType.TagFileGatewayArchive).Upload(data, path);
                  Logger.LogInformation($"Successfully uploaded Tag File {request.FileName}");
                }
                else
                {
                  Logger.LogInformation($"No S3 upload as NoArchiveOnInternalError set. Tag File {request.FileName}");
                }
            }

            if (internalProcessingError)
            {
              Logger.LogError($"{nameof(TagFileProcessExecutor)} InternalProcessingError {result.Code} {request.FileName} archiveFlag: {ArchiveOnInternalError}");
              return ContractExecutionResult.ErrorResult("Failed to connect to backend");
            }

            return result;
        }

        public static string GetS3Key(string tagFileName)
        {
            //Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
            //Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
            //Required folder structure is /<serial>--<machine name>/<serial--machine name--date>/<tagfile>
            //e.g. 0415J010SW--HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
            const string separator = "--";
            var parts = tagFileName.Split(new string[] { separator }, StringSplitOptions.None);
            if (parts.Length < 3)
            {
                return $"{INVALID_TAG_FILE_FOLDER}/{tagFileName}";
            }
            var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);
            //TCC org ID is not provided with direct submission from machines
            return $"{parts[0]}{separator}{parts[1]}/{nameWithoutTime}/{tagFileName}";
        }

        private bool IsInternalError(bool internalProcessingError, int resultCode)
        {
          // internalErrors can occur at any stage e.g. connecting to Trex/TFA/Project/CWS or internal to TRex
          //   todo keep an eye on any of these which may never succeed. Potential retryAttemptCount in S3 metadata
          if (
                // unable to connect to TRex
              internalProcessingError ||
              resultCode == ContractExecutionStatesEnum.InternalProcessingError || 
                // other internal e.g. TRex unable to connect to TFA/ProjectSvc/cws 
              resultCode == (int) TRexTagFileResultCode.TRexUnknownException ||
              resultCode == (int) TRexTagFileResultCode.TRexTfaException     || 
              resultCode == (int) TRexTagFileResultCode.TRexQueueSubmissionError ||
              resultCode == (int) TRexTagFileResultCode.TFAInternalServiceAccess || 
              resultCode == (int) TRexTagFileResultCode.CWSEndpointException  
             )
            return true;
          return false;
        }
    }
}
