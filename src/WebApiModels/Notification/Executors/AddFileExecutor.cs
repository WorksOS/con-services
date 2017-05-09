using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Notification.Models;
using System.Globalization;
using System.Text;
using ASNodeDecls;
using DesignProfilerDecls;
using TCCFileAccess;
using VLPDDecls;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.Notification.Executors
{
  /// <summary>
  /// Processes the request to add a file.
  /// Action taken depends on the file type.
  /// </summary>
  public class AddFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public AddFileExecutor(ILoggerFactory logger, IASNodeClient raptorClient, IFileRepository fileRepository) : base(logger, raptorClient, null, null, fileRepository)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AddFileExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        ProjectFileDescriptor request = item as ProjectFileDescriptor;
        ImportedFileTypeEnum fileType = GetFileType(request.File.fileName);
        //Only alignment files at present
        if (fileType != ImportedFileTypeEnum.Alignment)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
                "Unsupported file type"));
        }

        //Tell Raptor to update its cache
        //TODO: Does Raptor get the TCC file name with the UTC suffix or the original for surveyed surfaces?
        var result1 = raptorClient.UpdateCacheWithDesign(request.projectId.Value, request.File.fileName, 0, false);
        if (result1 != TDesignProfilerRequestResult.dppiOK)
        {
          //TODO: Use the ContractExecutionStates.DynamicAddwithOffset like AddTagProcessorErrorMessages
          //Talk to Dmitry - problem because Iwe have multiple sets of custom errors in this executor
          //Do we want to do AddErrorMessages and ClearDynamic multiple times here?
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Failed to update Raptor design cache"));
        }
   
        //Get PRJ file contents from Raptor
        string prjFile;
        var result2 = raptorClient.GetCoordinateSystemProjectionFile(request.projectId.Value,
          TVLPDDistanceUnits.vduMeters, out prjFile);
        if (result2 != TASNodeErrorStatus.asneOK)
        {
          //TODO: Need to add custom errors
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                           string.Format("Failed to get requested PRJ file with error: {0}.", ContractExecutionStates.FirstNameWithOffset((int)result2))));
        }
        bool success = CreateTransformFile(request.File, prjFile, GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX, ".PRJ");
        if (!success)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
             new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                 "Failed to create PRJ file"));
        }


        //Get GM_XFORM file contents from Raptor
        string haFile;
        var result3 = raptorClient.GetCoordinateSystemHorizontalAdjustmentFile("TODO: from Project MDM V4",
          request.projectId.Value, TVLPDDistanceUnits.vduMeters, out haFile);
        if (result3 != TASNodeErrorStatus.asneOK)
        {
          //TODO: Need to add custom errors
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                           string.Format("Failed to get requested GM_XFORM file with error: {0}.", ContractExecutionStates.FirstNameWithOffset((int)result2))));
        }
        success = CreateTransformFile(request.File, prjFile, GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX, ".GM_XFORM");
        if (!success)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
             new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                 "Failed to create GM_XFORM file"));
        }

        //Get alignment boundary as DXF file from Raptor

        //TODO: coord system file name returned in V4 Project MDM - need to update project proxy

        //Save these files to TCC

        //Generate DXF tiles

 
        return null;
      }
      finally
      {
      }

    }

    private ImportedFileTypeEnum GetFileType(string fileName)
    {

      string ext = Path.GetExtension(fileName).ToUpper();
      if (ext == ".DXF")
        return ImportedFileTypeEnum.Linework;
      if (ext == ".TTM")
      {
        var shortFileName = Path.GetFileNameWithoutExtension(fileName);
        var format = "yyyy-MM-ddTHH:mm:ssZ";
        DateTime dateTime;
        if (isDateTimeISO8601(shortFileName.Substring(shortFileName.Length - format.Length),format, out dateTime))
        {
          return ImportedFileTypeEnum.SurveyedSurface;   
        }
        return ImportedFileTypeEnum.DesignSurface;
      }
      if (ext == ".SVL")
        return ImportedFileTypeEnum.Alignment;
      if (ext == ".KML" || ext == ".KMZ")
        return ImportedFileTypeEnum.MobileLinework;
      if (ext == ".VCL" || ext == ".TMH")
        return ImportedFileTypeEnum.MassHaulPlan;

      //Reference surface does not have it's own file. It is an offset wrt an existing design surface.

      throw new ServiceException(HttpStatusCode.BadRequest,
         new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
             "Unsupported file type"));
    }

    private bool isDateTimeISO8601(string inputStringUTC, string format, out DateTime resultDateTimeUTC)
    {
      if (string.IsNullOrWhiteSpace(inputStringUTC))
      {
        resultDateTimeUTC = DateTime.MinValue;
        return false;
      }

      return DateTime.TryParseExact(inputStringUTC, format, new CultureInfo("en-US"), DateTimeStyles.AdjustToUniversal, out resultDateTimeUTC);
    }

    
    private bool CreateTransformFile(FileDescriptor fileDescr, string fileData, string suffix, string extension)
    {
      if (string.IsNullOrEmpty(fileData))
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Empty transform file contents"));
      }
      bool success = false;
      using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileData)))
      {
        //TODO: do we want this async?
        var response = fileRepo.PutFile(null, fileDescr.path, GeneratedFileName(fileDescr.fileName, suffix, extension), memoryStream, fileData.Length).Result;
        success = response.success == "success";//TODO: check if this is correct.
      }
      return success;
    }

    private string GeneratedFileName(string fileName, string suffix, string extension)
    {
      return Path.GetFileNameWithoutExtension(fileName) + suffix + extension;
    }
    

    public const string GENERATED_SURFACE_FILE_SUFFIX = "_Boundary$";
    public const string GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX = "_AlignmentCenterline$";
    private const string DESIGN_SUBGRID_INDEX_FILE_EXT = ".$DesignSubgridIndex$";


  }
}
