using System;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using TCCFileAccess;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Notification.Models;

namespace WebApiModels.Notification.Helpers
{
  /// <summary>
  /// Utilities for imported files
  /// </summary>
  public class FileUtils
  {
    /// <summary>
    /// Gets the file type for the file name according to its extension
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>THe file type</returns>
    public static ImportedFileTypeEnum GetFileType(string fileName)
    {
      string ext = Path.GetExtension(fileName).ToUpper();
      if (ext == ".DXF")
        return ImportedFileTypeEnum.Linework;
      if (ext == ".TTM")
      {
        return SurveyedSurfaceUtc(fileName).HasValue
          ? ImportedFileTypeEnum.SurveyedSurface
          : ImportedFileTypeEnum.DesignSurface;
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

    /// <summary>
    /// Gets the surveyed surface UTC from the file name if it has been appended
    /// </summary>
    /// <param name="fileName">The file name to extract the time stamp from</param>
    /// <returns>The surveyed surface UTC</returns>
    public static DateTime? SurveyedSurfaceUtc(string fileName)
    {
      var shortFileName = Path.GetFileNameWithoutExtension(fileName);
      var format = "yyyy-MM-ddTHH:mm:ssZ";
      DateTime dateTime;
      if (IsDateTimeISO8601(shortFileName.Substring(shortFileName.Length - format.Length), format, out dateTime))
        return dateTime;
      return null;
    }

    /// <summary>
    /// Determines if the string contains an ISO8601 date time
    /// </summary>
    /// <param name="inputStringUTC">The string to check</param>
    /// <param name="format">The format to use when checking</param>
    /// <param name="resultDateTimeUTC">The date time from the string</param>
    /// <returns>True if the date time was successfully extracted</returns>
    private static bool IsDateTimeISO8601(string inputStringUTC, string format, out DateTime resultDateTimeUTC)
    {
      if (string.IsNullOrWhiteSpace(inputStringUTC))
      {
        resultDateTimeUTC = DateTime.MinValue;
        return false;
      }

      return DateTime.TryParseExact(inputStringUTC, format, new CultureInfo("en-US"), DateTimeStyles.AdjustToUniversal, out resultDateTimeUTC);
    }

    /// <summary>
    /// Generates an associated file name for the file associated with the specified file
    /// </summary>
    /// <param name="fileName">THe original file name</param>
    /// <param name="suffix">The suffix to append to the file name for the associated file</param>
    /// <param name="extension">The extension to apply to the generated file name</param>
    /// <returns>The generated file name</returns>
    public static string GeneratedFileName(string fileName, string suffix, string extension)
    {
      return Path.GetFileNameWithoutExtension(fileName) + suffix + extension;
    }

    /// <summary>
    /// The suffix to apply to a generated file name for an associated file
    /// </summary>
    /// <param name="fileType">The type of file for which the associated file will be generated</param>
    /// <returns>The suffix</returns>
    public static string GeneratedFileSuffix(ImportedFileTypeEnum fileType)
    {
      switch (fileType)
      {
          case ImportedFileTypeEnum.Linework: return string.Empty;
          case ImportedFileTypeEnum.Alignment: return GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX;
          case ImportedFileTypeEnum.DesignSurface: return GENERATED_SURFACE_FILE_SUFFIX;
      }
      return string.Empty;
    }

    /// <summary>
    /// The generated folder name for DXF tiles to be stored in
    /// </summary>
    /// <param name="dxfFileName">THe DXF file to which the tiles belong</param>
    /// <returns>The tile folder name</returns>
    public static string TilesFolderWithSuffix(string dxfFileName)
    {
      return string.Format("{0}{1}", dxfFileName, GENERATED_TILE_FOLDER_SUFFIX);
    }

  

    private const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    private const string GENERATED_SURFACE_FILE_SUFFIX = "_Boundary$";
    private const string GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX = "_AlignmentCenterline$";
    public const string DESIGN_SUBGRID_INDEX_FILE_EXT = ".$DesignSubgridIndex$";

    public const string DXF_FILE_EXTENSION = ".DXF";
    public const string PROJECTION_FILE_EXTENSION = ".PRJ";
    public const string HORIZONTAL_ADJUSTMENT_FILE_EXTENSION = ".GM_XFORM";

  }
}
