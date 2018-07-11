using ASNodeDecls;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using AddFileResult = VSS.Productivity3D.WebApi.Models.Notification.Models.AddFileResult;

namespace VSS.Productivity3D.WebApiModels.Notification.Executors
{
  /// <summary>
  /// Processes the request to add a file.
  /// Action taken depends on the file type.
  /// </summary>
  public class AddFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AddFileExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
      RaptorResult.AddDesignProfileErrorMessages(ContractExecutionStates);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        ZoomRangeResult zoomResult = new ZoomRangeResult();
        ProjectFileDescriptor request = item as ProjectFileDescriptor;
        ImportedFileType fileType = request.FileType;
        log.LogDebug($"FileType is: {fileType}");

        //Tell Raptor to update its cache. 
        //Note: surveyed surface file names are the TCC one including the surveyed UTC in the file name
        if (fileType == ImportedFileType.Alignment || 
            fileType == ImportedFileType.DesignSurface ||
            fileType == ImportedFileType.SurveyedSurface)
        {
          log.LogDebug("Updating Raptor design cache");

          var result1 = raptorClient.UpdateCacheWithDesign(request.ProjectId.Value, request.File.fileName, 0, false);
          if (result1 != TDesignProfilerRequestResult.dppiOK)
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
              ContractExecutionStatesEnum.FailedToGetResults,
              string.Format("Failed to update Raptor design cache with error: {0}",
                ContractExecutionStates.FirstNameWithOffset((int)result1))));
          }
        }

        switch (fileType)
        {
          case ImportedFileType.Linework:
          case ImportedFileType.DesignSurface:
          case ImportedFileType.Alignment:
            var suffix = FileUtils.GeneratedFileSuffix(fileType);
            //Get PRJ file contents from Raptor
            log.LogDebug("Getting projection file from Raptor");
            var dxfUnitsType = fileType == ImportedFileType.Linework || fileType == ImportedFileType.Alignment
              ? (TVLPDDistanceUnits)request.DXFUnitsType
              : TVLPDDistanceUnits.vduMeters; //always metric for design surface and alignment as we generate the DXF file.

            var result2 = raptorClient.GetCoordinateSystemProjectionFile(request.ProjectId.Value,
              dxfUnitsType, out string prjFile);
            if (result2 != TASNodeErrorStatus.asneOK)
            {
              //We need gracefully fail here as the file may be imported to an empty datamodel
              log.LogWarning("Failed to get requested " + FileUtils.PROJECTION_FILE_EXTENSION + " file with error: {0}.",
                ContractExecutionStates.FirstNameWithOffset((int)result2));

              return new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Add file notification partially successful - no tiles can be generated")
                { MinZoomLevel = 0, MaxZoomLevel = 0 };

              /*throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
              ContractExecutionStatesEnum.FailedToGetResults,
              string.Format("Failed to get requested " + FileUtils.PROJECTION_FILE_EXTENSION + " file with error: {0}.",
                ContractExecutionStates.FirstNameWithOffset((int)result2))));*/

            }
            //Note: Cannot have async void therefore bool result from method. However, failure handled inside method so ignore return value here.
            await CreateTransformFile(request.ProjectId.Value, request.File, prjFile, suffix, FileUtils.PROJECTION_FILE_EXTENSION);

            //Get GM_XFORM file contents from Raptor
            log.LogDebug("Getting horizontal adjustment file from Raptor");

            var result3 = raptorClient.GetCoordinateSystemHorizontalAdjustmentFile(request.CoordSystemFileName,
              request.ProjectId.Value, dxfUnitsType, out string haFile);
            if (result3 != TASNodeErrorStatus.asneOK)
            {
              log.LogWarning(string.Format(
                "Failed to get requested " + FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION + " file with error: {0}.",
                ContractExecutionStates.FirstNameWithOffset((int)result2)));
              return new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Add file notification partially successful. Can not create horizontal adjustment - no tiles can be generated")
                { MinZoomLevel = 0, MaxZoomLevel = 0 };

              /* throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
               ContractExecutionStatesEnum.FailedToGetResults,
               string.Format(
                 "Failed to get requested " + FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION + " file with error: {0}.",
                 ContractExecutionStates.FirstNameWithOffset((int)result2))));*/
            }
            //An empty string means there is no horizontal adjustment in coordinate system so no file to create
            if (haFile != string.Empty)
            {
              await CreateTransformFile(request.ProjectId.Value, request.File, haFile, suffix,
                FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION);
            }

            if (fileType != ImportedFileType.Linework)
            {
              //Get alignment or surface boundary as DXF file from Raptor
              if (!await CreateDxfFile(request.ProjectId.Value, request.File, suffix, request.DXFUnitsType))
              {
                //We need gracefully fail here as the file may be imported to an empty datamodel
                log.LogWarning("Failed to get requested " + FileUtils.DXF_FILE_EXTENSION);

                return new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Add file notification partially successful. Can not create DXF - no tiles can be generated")
                  { MinZoomLevel = 0, MaxZoomLevel = 0 };
              }
            }
            //Calculate the zoom range
            string generatedName = FileUtils.GeneratedFileName(request.File.fileName, suffix, FileUtils.DXF_FILE_EXTENSION);
            var fullGeneratedName = string.Format("{0}/{1}", request.File.path, generatedName);
            zoomResult = await tileGenerator.CalculateTileZoomRange(request.File.filespaceId, fullGeneratedName).ConfigureAwait(false); 
            //Generate DXF tiles
            await tileGenerator.CreateDxfTiles(request.ProjectId.Value, request.File, suffix, zoomResult, false).ConfigureAwait(false);
            break;
          case ImportedFileType.SurveyedSurface:
            log.LogDebug("Storing ground surface file in Raptor");
            DesignDescriptor dd = DesignDescriptor.CreateDesignDescriptor(request.FileId, request.File, 0.0);
            ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args = ASNode.GroundSurface.RPC.__Global
              .Construct_GroundSurface_Args(
                request.ProjectId.Value,
                request.FileId,
                FileUtils.SurveyedSurfaceUtc(request.File.fileName).Value,
                RaptorConverters.DesignDescriptor(dd)
              );

            if (!raptorClient.StoreGroundSurfaceFile(args))
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  "Failed to store ground surface file"));
            }

            break;
        }

        return new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Add file notification successful")
        { MinZoomLevel = zoomResult.minZoom, MaxZoomLevel = zoomResult.maxZoom };
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

    }

    /// <summary>
    /// Creates an associated transformation file
    /// </summary>
    /// <param name="projectId">The id of the project to which the file belongs</param>
    /// <param name="fileDescr">The original file for which the associated file is created</param>
    /// <param name="fileData">The contents of the associated file</param>
    /// <param name="suffix">The suffix applied to the file name to get the generated file name</param>
    /// <param name="extension">The file extension of the generated file</param>
    private async Task<bool> CreateTransformFile(long projectId, FileDescriptor fileDescr, string fileData, string suffix, string extension)
    {
      log.LogDebug("Creating {0} transform file for {1}", extension, fileDescr.fileName);

      if (string.IsNullOrEmpty(fileData))
      {
        throw new ServiceException(HttpStatusCode.BadRequest, 
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Empty transform file contents"));
      }
      using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileData)))
      {
        return await PutFile(projectId, fileDescr, suffix, extension, memoryStream, fileData.Length);
      }
    }

    /// <summary>
    /// Creates an associated DXF file
    /// </summary>
    /// <param name="projectId">The id of the project to which the file belongs</param>
    /// <param name="fileDescr">The original file for which the associated file is created</param>
    /// <param name="suffix">The suffix applied to the file name to get the generated file name</param>
    /// <param name="userUnits">The user units preference</param>
    private async Task<bool> CreateDxfFile(long projectId, FileDescriptor fileDescr, string suffix, DxfUnitsType userUnits)
    {
      const double ImperialFeetToMetres = 0.3048;
      const double USFeetToMetres = 0.304800609601;

      //NOTE: For alignment files only (not surfaces), there are labels generated as part of the DXF file.
      //They need to be in the user units.
      double interval;
      TVLPDDistanceUnits raptorUnits;
      switch (userUnits)
      {
        case DxfUnitsType.ImperialFeet:
          raptorUnits = TVLPDDistanceUnits.vduImperialFeet;
          interval = 300 * ImperialFeetToMetres;
          break;

        case DxfUnitsType.Meters:
          raptorUnits = TVLPDDistanceUnits.vduMeters;
          interval = 100;
          break;
        case DxfUnitsType.UsSurveyFeet:
        default:
          raptorUnits = TVLPDDistanceUnits.vduUSSurveyFeet;
          interval = 300 * USFeetToMetres;
          break;
      }

      log.LogDebug("Getting DXF design boundary from Raptor");

      raptorClient.GetDesignBoundary(
        DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args
        (projectId,
          fileDescr.DesignDescriptor(configStore, log, 0, 0),
          DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtDXF,
          interval, raptorUnits,0), out var memoryStream, out var designProfilerResult);

      if (memoryStream != null)
      {
        return await PutFile(projectId, fileDescr, suffix, FileUtils.DXF_FILE_EXTENSION, memoryStream, memoryStream.Length);
      }
      else
      {
        log.LogWarning("Failed to generate DXF boundary for file {0} for project {1}. Raptor error {2}", fileDescr.fileName, projectId, designProfilerResult);

        //We need gracefully fail here as the file may be imported to an empty datamodel

        return false;

        /*throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
          ContractExecutionStatesEnum.FailedToGetResults,
          string.Format("Failed to create " + FileUtils.DXF_FILE_EXTENSION + " file with error: {0}",
            ContractExecutionStates.FirstNameWithOffset((int)designProfilerResult))));*/
      }
    }

    /// <summary>
    /// Saves an associated file to TCC
    /// </summary>
    /// <param name="projectId">The id of the project to which the file belongs</param>
    /// <param name="fileDescr">The original file for which the associated file is created</param>
    /// <param name="suffix">The suffix applied to the file name to get the generated file name</param>
    /// <param name="extension">The file extension of the generated file</param>
    /// <param name="memoryStream">The contents of the associated file</param>
    /// <param name="length">The length of the contents</param>
    private async Task<bool> PutFile(long projectId, FileDescriptor fileDescr, string suffix, string extension, MemoryStream memoryStream, long length)
    {
      //TODO: do we want this async?
      var generatedName = FileUtils.GeneratedFileName(fileDescr.fileName, suffix, extension);
      log.LogDebug("Saving file {0} in TCC", generatedName);
      if (! await fileRepo.PutFile(fileDescr.filespaceId, fileDescr.path,
        generatedName, memoryStream, length))
      {
        log.LogWarning("Failed to save file {0} for project {1}", generatedName, projectId);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Failed to create associated file " + generatedName));
      }
      return true;
    }
  }
}
