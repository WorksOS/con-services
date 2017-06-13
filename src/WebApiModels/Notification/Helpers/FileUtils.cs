﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    public static ImportedFileType GetFileType(string fileName)
    {
      string ext = Path.GetExtension(fileName).ToUpper();
      if (ext == ".DXF")
        return ImportedFileType.Linework;
      if (ext == ".TTM")
      {
        return SurveyedSurfaceUtc(fileName).HasValue
          ? ImportedFileType.SurveyedSurface
          : ImportedFileType.DesignSurface;
      }
      if (ext == ".SVL")
        return ImportedFileType.Alignment;
      if (ext == ".KML" || ext == ".KMZ")
        return ImportedFileType.MobileLinework;
      if (ext == ".VCL" || ext == ".TMH")
        return ImportedFileType.MassHaulPlan;

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
      DateTime dateTime = shortFileName.Substring(shortFileName.Length - format.Length).IsDateTimeISO8601(format);
      return dateTime == DateTime.MinValue ? (DateTime?)null : dateTime;
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
    public static string GeneratedFileSuffix(ImportedFileType fileType)
    {
      switch (fileType)
      {
          case ImportedFileType.Linework: return string.Empty;
          case ImportedFileType.Alignment: return GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX;
          case ImportedFileType.DesignSurface: return GENERATED_SURFACE_FILE_SUFFIX;
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

    /// <summary>
    /// The full folder name of where the tiles are stored
    /// </summary>
    /// <param name="path">The full path of where the DXF file is located</param>
    /// <param name="generatedName">The DXF file name which is generated for an alignment or design file</param>
    /// <returns>The full path of the tile folder</returns>
    public static string TilePath(string path, string generatedName)
    {
      string tileFolder = TilesFolderWithSuffix(generatedName);
      return string.Format("{0}/{1}", path, tileFolder);
    }

    /// <summary>
    /// The path to the zoom folder name for the specified zoom level
    /// </summary>
    /// <param name="tilePath">The path to the folder where the tiles are stored</param>
    /// <param name="zoomLevel">The zoom level</param>
    /// <returns>The zoom path</returns>
    public static string ZoomPath(string tilePath, int zoomLevel)
    {
      return string.Format("{0}/Z{1}", tilePath, zoomLevel);
    }



    private const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    private const string GENERATED_SURFACE_FILE_SUFFIX = "_Boundary$";
    private const string GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX = "_AlignmentCenterline$";
    public const string DESIGN_SUBGRID_INDEX_FILE_EXT = ".$DesignSubgridIndex$";

    public const string DXF_FILE_EXTENSION = ".DXF";
    public const string PROJECTION_FILE_EXTENSION = ".PRJ";
    public const string HORIZONTAL_ADJUSTMENT_FILE_EXTENSION = ".GM_XFORM";

  }

  public static class DateTimeextensions
  {
    /// <summary>
    /// Determines if the string contains an ISO8601 date time
    /// </summary>
    /// <param name="inputStringUtc">The string to check</param>
    /// <param name="format">The format to use when checking</param>
    /// <returns>The date time from the string if ISO8601 else DateTime.MinDate</returns>
    public static DateTime IsDateTimeISO8601(this string inputStringUtc, string format)
    {
      DateTime utcDate = DateTime.MinValue;
      if (!string.IsNullOrWhiteSpace(inputStringUtc))
      {
        if (!DateTime.TryParseExact(inputStringUtc, format, new CultureInfo("en-US"), DateTimeStyles.AdjustToUniversal,
          out utcDate))
        {
          utcDate = DateTime.MinValue;
        }
      }
      return utcDate;
    }
  }
}
