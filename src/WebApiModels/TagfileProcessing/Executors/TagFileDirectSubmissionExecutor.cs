using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting direct submitted TAG files to Raptor.
  /// </summary>
  public class TagFileDirectSubmissionExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as TagFileRequestLegacy;

        var data = new MemoryStream(request.Data);
        var returnResult = tagProcessor.ProjectDataServerTAGProcessorClient()
                                       .SubmitTAGFileToTAGFileProcessor
                                       (request.FileName,
                                         data,
                                         request.ProjectId ?? -1, 0, 0, request.MachineId ?? -1,
                                         request.Boundary != null
                                           ? RaptorConverters.convertWGS84Fence(request.Boundary)
                                           : TWGS84FenceContainer.Null(), request.TccOrgId);

        ArchiveFile(returnResult, data, request.FileName, request.TccOrgId);

        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(returnResult));
      }
      catch (Exception ex)
      {
        log.LogError($"TagFileDirectSubmissionExecutor: {ex.Message}");
        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprUnknown));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder struucture in the tagfile harvester.
    /// </summary>
    /// <param name="resultCode">Code from Raptor</param>
    /// <param name="data">Tagfile contents to archive</param>
    /// <param name="tagFileName">Tagfile name</param>
    /// <param name="tccOrgId">ID of TCC organization who owns the tagfile</param>
    private void ArchiveFile(TTAGProcServerProcessResult resultCode, Stream data, string tagFileName, string tccOrgId)
    {
      string folderName = null;
      switch (resultCode)
      {
        case TTAGProcServerProcessResult.tpsprOK:
          folderName = configStore.GetValueString("TCCSynchProductionDataArchivedFolder") ?? "Production-Data (Archived)";
          break;

        case TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel:
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
          folderName = configStore.GetValueString("TCCSynchProjectBoundaryIssueFolder") ?? "Project Boundary (Issue)";
          break;

        case TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions:
        case TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine:
        case TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine:
          folderName = configStore.GetValueString("TCCSynchSubscriptionIssueFolder") ?? "Subscription (Issue)";
          break;

        case TTAGProcServerProcessResult.tpsprTFAServiceError:
          log.LogError("TFA is likely down for {0} org {1}", tagFileName, tccOrgId);
          break;

        default:
          folderName = configStore.GetValueString("TCCSynchOtherIssueFolder") ?? "Other... (Issue)";
          break;
      }

      if (!string.IsNullOrEmpty(folderName))
      {
        log.LogDebug("Moving file {0} for org {1} to {2} folder", tagFileName, tccOrgId, folderName);
        transferProxy.Upload(data, GetS3Key(tagFileName, folderName, tccOrgId));
      }
    }

    /// <summary>
    /// Gets the key in the S3 bucket to save the tagfile under.
    /// </summary>
    /// <param name="tagFileName">Name of tagfile</param>
    /// <param name="archiveFolder">Archive folder</param>
    /// <param name="tccOrgId">TCC organization who ows the tagfile</param>
    /// <returns></returns>
    private string GetS3Key(string tagFileName, string archiveFolder, string tccOrgId)
    {
      //Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
      //Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
      //Required folder structure is <TCC org id>/<machine name>/<archive folder>/<display--machine name--date>/<tagfile>
      //e.g. HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
      string[] parts = tagFileName.Split(new string[] {"--"}, StringSplitOptions.None);
      var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);
      //TCC org ID is not always provided
      var prefix = string.IsNullOrEmpty(tccOrgId) ? string.Empty : $"{tccOrgId}/";
      return $"{prefix}{parts[1]}/{archiveFolder}/{nameWithoutTime}/{tagFileName}";
    }
  }
}
