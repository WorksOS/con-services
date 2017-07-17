using System;
using System.Globalization;
using System.IO;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TCCFileAccess
{

  public class TCCFile
  {
    private const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    private const string GENERATED_SURFACE_FILE_SUFFIX = "_Boundary$";
    private const string GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX = "_AlignmentCenterline$";
    public const string DESIGN_SUBGRID_INDEX_FILE_EXT = ".$DesignSubgridIndex$";

    public const string DXF_FILE_EXTENSION = ".DXF";
    public const string PROJECTION_FILE_EXTENSION = ".PRJ";
    public const string HORIZONTAL_ADJUSTMENT_FILE_EXTENSION = ".GM_XFORM";

    public ImportedFileType FileType { get; private set; }
    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public DateTime? SurveyedSurfaceTimestamp { get; private set; } = null;
    public string TilesPath => TilePath(FilePath, GeneratedName);

    /// <summary>
    /// Gets the name of the generated in TCC.
    /// </summary>
    /// <value>
    /// The name of the generated.
    /// </value>
    public string GeneratedName => Path.GetFileNameWithoutExtension(FileName) + GeneratedFileSuffix(FileType) +
                                   DXF_FILE_EXTENSION;

    public TCCFile(ImportedFileType fileType, string fileName, string path)
    {
      FileType = fileType;
      FileName = fileName;
      FilePath = path;
      if (fileType != GetFileType(fileName))
        throw new ArgumentException("File extension does not match file type",
          $"Filename: {fileName} Filetype: {fileType}");
      if (fileType == ImportedFileType.SurveyedSurface)
        SurveyedSurfaceTimestamp = SurveyedSurfaceUtc(fileName);
    }

    public TCCFile(string fileName, string path)
    {
      FileName = fileName;
      FilePath = path;
      FileType = GetFileType(fileName);
      if (FileType == ImportedFileType.SurveyedSurface)
        SurveyedSurfaceTimestamp = SurveyedSurfaceUtc(fileName);
    }

    /// <summary>
    /// Gets the name of the tile file.
    /// </summary>
    /// <param name="zoomLevel">The zoom level.</param>
    /// <param name="topLeftTileY">The top left tile y.</param>
    /// <param name="topLeftTileX">The top left tile x.</param>
    /// <returns></returns>
    public string GetTileFileName(int zoomLevel, int topLeftTileY, int topLeftTileX)
    {
      return string.Format("{0}/{1}/{2}.png", ZoomPath(TilePath(FilePath, GeneratedName), zoomLevel), topLeftTileY,
        topLeftTileX);
    }

    /// <summary>
    /// Gets the file type for the file name according to its extension
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>THe file type</returns>
    private ImportedFileType GetFileType(string fileName)
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
      throw new ArgumentException("Unsupported file type", fileName);
    }


    /// <summary>
    /// Gets the surveyed surface UTC from the file name if it has been appended
    /// </summary>
    /// <param name="fileName">The file name to extract the time stamp from</param>
    /// <returns>The surveyed surface UTC</returns>
    private DateTime? SurveyedSurfaceUtc(string fileName)
    {
      var shortFileName = Path.GetFileNameWithoutExtension(fileName);
      var format = "yyyy-MM-ddTHH:mm:ssZ";
      if (shortFileName.Length <= format.Length)
        return (DateTime?) null;
      DateTime dateTime = shortFileName.Substring(shortFileName.Length - format.Length).IsDateTimeISO8601(format);
      return dateTime == DateTime.MinValue ? (DateTime?) null : dateTime;
    }

    /// <summary>
    /// The generated folder name for DXF tiles to be stored in
    /// </summary>
    /// <param name="dxfFileName">THe DXF file to which the tiles belong</param>
    /// <returns>The tile folder name</returns>
    private string TilesFolderWithSuffix(string dxfFileName)
    {
      return String.Format("{0}{1}", dxfFileName, GENERATED_TILE_FOLDER_SUFFIX);
    }

    /// <summary>
    /// The full folder name of where the tiles are stored
    /// </summary>
    /// <param name="path">The full path of where the DXF file is located</param>
    /// <param name="generatedName">The DXF file name which is generated for an alignment or design file</param>
    /// <returns>The full path of the tile folder</returns>
    private string TilePath(string path, string generatedName)
    {
      string tileFolder = TilesFolderWithSuffix(generatedName);
      return String.Format("{0}/{1}", path, tileFolder);
    }

    /// <summary>
    /// The suffix to apply to a generated file name for an associated file
    /// </summary>
    /// <param name="fileType">The type of file for which the associated file will be generated</param>
    /// <returns>The suffix</returns>
    private string GeneratedFileSuffix(ImportedFileType fileType)
    {
      switch (fileType)
      {
        case ImportedFileType.Linework: return string.Empty;
        case ImportedFileType.Alignment: return GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX;
        case ImportedFileType.DesignSurface: return GENERATED_SURFACE_FILE_SUFFIX;
      }
      return string.Empty;
    }

    public string GetZoomPath(int zoomLevel)
    {
      return ZoomPath(TilesPath, zoomLevel);
    }

    /// <summary>
    /// The path to the zoom folder name for the specified zoom level
    /// </summary>
    /// <param name="tilePath">The path to the folder where the tiles are stored</param>
    /// <param name="zoomLevel">The zoom level</param>
    /// <returns>The zoom path</returns>
    private string ZoomPath(string tilePath, int zoomLevel)
    {
      return string.Format("{0}/Z{1}", tilePath, zoomLevel);
    }

    /// <summary>
    /// Extracts the full name of the filename from tile filename.
    /// </summary>
    /// <param name="fullTileName">The full path and name of the tile file.</param>
    /// <returns>The base file name to which the tiles belong</returns>
    /// <exception cref="System.ArgumentException"></exception>
    public static string ExtractFileNameFromTileFullName(string fullTileName)
    {
      if (!fullTileName.Contains(GENERATED_TILE_FOLDER_SUFFIX))
        throw new ArgumentException($"Invalid fullname - no expected suffix {fullTileName}");
      var filename = fullTileName.Split(new string[] {GENERATED_TILE_FOLDER_SUFFIX}, StringSplitOptions.None)[0];
      return filename;
    }

    /// <summary>
    /// Determines if the file is to be cached. Only tiles, stored as PNG files, are cached.
    /// </summary>
    /// <param name="filename">The file name to check</param>
    /// <returns>True if the file is to be cached otherwise false</returns>
    public static bool FileCacheable(string filename)
    {
      return filename.Contains(GENERATED_TILE_FOLDER_SUFFIX) && filename.Contains(".png");
    }
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