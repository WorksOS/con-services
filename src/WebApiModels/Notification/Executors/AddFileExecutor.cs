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
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
        ProjectFileDescriptor request = item as ProjectFileDescriptor;
        ImportedFileType fileType = request.FileType;//FileUtils.GetFileType(request.File.fileName);
        log.LogDebug($"FileType is: {fileType}");

        //Tell Raptor to update its cache. 
        //Note: surveyed surface file names are the TCC one including the surveyed UTC in the file name
        if (fileType == ImportedFileType.Alignment || 
            fileType == ImportedFileType.DesignSurface ||
            fileType == ImportedFileType.SurveyedSurface)
        {
          log.LogDebug("Updating Raptor design cache");

          var result1 = raptorClient.UpdateCacheWithDesign(request.projectId.Value, request.File.fileName, 0, false);
          if (result1 != TDesignProfilerRequestResult.dppiOK)
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
              ContractExecutionStatesEnum.FailedToGetResults,
              string.Format("Failed to update Raptor design cache with error: {0}",
                ContractExecutionStates.FirstNameWithOffset((int)result1))));
          }
        }

        if (fileType == ImportedFileType.Linework ||
            fileType == ImportedFileType.DesignSurface ||
            fileType == ImportedFileType.Alignment)
        {
          var suffix = FileUtils.GeneratedFileSuffix(fileType);
          //Get PRJ file contents from Raptor
          log.LogDebug("Getting projection file from Raptor");
          var dxfUnitsType = fileType == ImportedFileType.Linework
            ? (TVLPDDistanceUnits)request.DXFUnitsType
            : TVLPDDistanceUnits.vduMeters; //always metric for design surface and alignment as we generate the DXF file.

          string prjFile;
          var result2 = raptorClient.GetCoordinateSystemProjectionFile(request.projectId.Value,
            dxfUnitsType, out prjFile);
          if (result2 != TASNodeErrorStatus.asneOK)
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
              ContractExecutionStatesEnum.FailedToGetResults,
              string.Format("Failed to get requested " + FileUtils.PROJECTION_FILE_EXTENSION + " file with error: {0}.",
                ContractExecutionStates.FirstNameWithOffset((int)result2))));
          }
          //Note: Cannot have async void therefore bool result from method. However, failure handled inside method so ignore return value here.
          await CreateTransformFile(request.projectId.Value, request.File, prjFile, suffix, FileUtils.PROJECTION_FILE_EXTENSION);

          //Get GM_XFORM file contents from Raptor
          log.LogDebug("Getting horizontal adjustment file from Raptor");
          string haFile;
          var result3 = raptorClient.GetCoordinateSystemHorizontalAdjustmentFile(request.CoordSystemFileName,
            request.projectId.Value, dxfUnitsType, out haFile);
          if (result3 != TASNodeErrorStatus.asneOK)
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
              ContractExecutionStatesEnum.FailedToGetResults,
              string.Format(
                "Failed to get requested " + FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION + " file with error: {0}.",
                ContractExecutionStates.FirstNameWithOffset((int)result2))));
          }
          await CreateTransformFile(request.projectId.Value, request.File, haFile, suffix, FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION);
     

          if (fileType != ImportedFileType.Linework)
          {
            //Get alignment or surface boundary as DXF file from Raptor
            await CreateDxfFile(request.projectId.Value, request.File, suffix, request.DXFUnitsType);    
          }
          //Generate DXF tiles
          await tileGenerator.CreateDxfTiles(request.projectId.Value, request.File, suffix, false).ConfigureAwait(false);
        }

        else if (fileType == ImportedFileType.SurveyedSurface)
        {
          log.LogDebug("Storing ground surface file in Raptor");
          DesignDescriptor dd = Productivity3D.Common.Models.DesignDescriptor.CreateDesignDescriptor(request.FileId, request.File, 0.0);
          ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args = ASNode.GroundSurface.RPC.__Global
            .Construct_GroundSurface_Args(
              request.projectId.Value,
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
        }

        return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Add file notification successful");
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
          raptorUnits = VLPDDecls.TVLPDDistanceUnits.vduImperialFeet;
          interval = 300 * ImperialFeetToMetres;
          break;

        case DxfUnitsType.Meters:
          raptorUnits = VLPDDecls.TVLPDDistanceUnits.vduMeters;
          interval = 100;
          break;
        case DxfUnitsType.UsSurveyFeet:
        default:
          raptorUnits = VLPDDecls.TVLPDDistanceUnits.vduUSSurveyFeet;
          interval = 300 * USFeetToMetres;
          break;
      }

      MemoryStream memoryStream;
      TDesignProfilerRequestResult designProfilerResult = TDesignProfilerRequestResult.dppiUnknownError;
      log.LogDebug("Getting DXF design boundary from Raptor");

      raptorClient.GetDesignBoundary(
        DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args
        (projectId,
          fileDescr.DesignDescriptor(configStore, log, 0, 0),
          DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtDXF,
          interval, raptorUnits,0), out memoryStream, out designProfilerResult);

      if (memoryStream != null)
      {
        return await PutFile(projectId, fileDescr, suffix, FileUtils.DXF_FILE_EXTENSION, memoryStream, memoryStream.Length);
      }
      else
      {
        log.LogWarning("Failed to generate DXF boundary for file {0} for project {1}. Raptor error {2}", fileDescr.fileName, projectId, designProfilerResult);
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
          ContractExecutionStatesEnum.FailedToGetResults,
          string.Format("Failed to create " + FileUtils.DXF_FILE_EXTENSION + " file with error: {0}",
            ContractExecutionStates.FirstNameWithOffset((int)designProfilerResult))));
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
