using System;
using System.IO;
using System.Linq;
using VSS.Common.Abstractions.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.DataOcean.Client
{
  public class DataOceanFileUtil
  {
    public const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    public const string GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX = "_AlignmentCenterline$";
    public const string DXF_FILE_EXTENSION = ".dxf";
    public static readonly string[] SUPPORTED_FILE_EXTENSIONS = { ".tif", ".tiff", DXF_FILE_EXTENSION };

    public string FileName { get; }
    public string FilePath { get; }
    public string FullFileName { get; }
    public string TilesPath => TilePath();
    public string TilesMetadataFileName => TileMetadataFileName();
    public string GeneratedTilesFolder => TilesFolderWithSuffix();

    public DataOceanFileUtil(string fullFileName)
    { 
      FileName = Path.GetFileName(fullFileName);
      FilePath = Path.GetDirectoryName(fullFileName)
                     ?.Replace(Path.DirectorySeparatorChar, DataOceanUtil.PathSeparator);

      FullFileName = $"{FilePath}{DataOceanUtil.PathSeparator}{FileName}";
      var extension = Path.GetExtension(FileName);

      if (!SUPPORTED_FILE_EXTENSIONS.Contains(extension, StringComparer.OrdinalIgnoreCase))
      {
        throw new ArgumentException($"Only DXF and GeoTIFF files are supported. {FileName} is not a DXF or GeoTIFF file.");
      }
    }

    /// <summary>
    /// The generated folder name for DXF tiles to be stored in
    /// </summary>
    /// <returns>The tile folder name</returns>
    private string TilesFolderWithSuffix() => $"{FilePath}{DataOceanUtil.PathSeparator}{Path.GetFileNameWithoutExtension(FileName)}{GENERATED_TILE_FOLDER_SUFFIX}";

    /// <summary>
    /// The top level folder of all tile files
    /// </summary>
    private string BaseTilePath() => $"{TilesFolderWithSuffix()}/tiles";

    /// <summary>
    /// The full folder name of where the tiles are stored
    /// </summary>
    /// <returns>The full path of the tile folder</returns>
    private string TilePath() => $"{BaseTilePath()}/xyz";

    /// <summary>
    /// The full name of the tiles metadata file
    /// </summary>
    /// <returns>The full name of the tile metadata file</returns>
    public string TileMetadataFileName()
    {
      var name = DXF_FILE_EXTENSION.Equals(Path.GetExtension(FileName), StringComparison.OrdinalIgnoreCase) ? "tiles" : "xyz";
      return $"{BaseTilePath()}/{name}.json";
    }

    /// <summary>
    /// Gets the name of the tile file.
    /// </summary>
    public string GetTileFileName(int zoomLevel, int topLeftTileY, int topLeftTileX) => $"{ZoomPath(zoomLevel)}/{topLeftTileY}/{topLeftTileX}.png";

    /// <summary>
    /// The path to the zoom folder name for the specified zoom level
    /// </summary>
    private string ZoomPath(int zoomLevel) => $"{TilePath()}/{zoomLevel}";

    /// <summary>
    /// Extracts the full name of the filename from tile filename.
    /// </summary>
    /// <param name="fullTileName">The full path and name of the tile file.</param>
    /// <returns>The base file name to which the tiles belong</returns>
    public static string ExtractBaseFileNameFromTileFullName(string fullTileName) => ExtractNameFromTileFullName(fullTileName, 0);

    /// <summary>
    /// Extracts the multipart path and file name of the tile. This is the path from the generated folder name
    /// down to the actual tile file name.
    /// </summary>
    public static string ExtractTileNameFromTileFullName(string fullTileName) => ExtractNameFromTileFullName(fullTileName, 1);

    /// <summary>
    /// Extracts either the base path or multipart path for the tile
    /// </summary>
    private static string ExtractNameFromTileFullName(string fullTileName, int indx)
    {
      if (!fullTileName.Contains(GENERATED_TILE_FOLDER_SUFFIX))
        throw new ArgumentException($"Invalid fullname - no expected suffix {fullTileName}");

      return fullTileName.Split(new[] { GENERATED_TILE_FOLDER_SUFFIX }, StringSplitOptions.None)[indx];
    }

    /// <summary>
    /// Determines if the file is to be cached. Only tiles, stored as PNG files, are cached.
    /// </summary>
    /// <param name="filename">The file name to check</param>
    /// <returns>True if the file is to be cached otherwise false</returns>
    public static bool FileCacheable(string filename) => filename.Contains(GENERATED_TILE_FOLDER_SUFFIX) && filename.Contains(".png");

    /// <summary>
    /// Gets the generated DXF file name
    /// </summary>
    public static string GeneratedFileName(string fileName, ImportedFileType fileType)
    {
      if (fileType == ImportedFileType.Alignment)
      {
        return $"{Path.GetFileNameWithoutExtension(fileName)}{GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX}{DXF_FILE_EXTENSION}";
      }

      if (fileType == ImportedFileType.Linework || fileType == ImportedFileType.GeoTiff)
      {
        return fileName;
      }
      throw new ArgumentException($"{fileName} is not a DXF, GeoTIFF or alignment file.");
    }

    /// <summary>
    /// Constructs the file name that the file is stored with in DataOcean
    /// </summary>
    public static string DataOceanFileName(string fileName, bool includeSurveyedUtc, Guid fileUid, DateTime? surveyedUtc)
    {
      //DataOcean doesn't handle Japanese characters so use fileUid as file name.
      //Coordinate system files use the projectUid and generated files for alignment center lines use alignmentUid
      var dataOceanFileName = $"{fileUid}{Path.GetExtension(fileName)}";
      //TODO: DataOcean has versions of files. We should leverage that rather than appending surveyed UTC to file name.
      if (includeSurveyedUtc && surveyedUtc != null) // validation should prevent this
        dataOceanFileName = dataOceanFileName.IncludeSurveyedUtcInName(surveyedUtc.Value);
      return dataOceanFileName;
    }

    /// <summary>
    /// Construct the path in DataOcean.
    /// </summary>
    public static string DataOceanPath(string rootFolder, string customerUid, string projectUid)
    {
      return string.Join(DataOceanUtil.PathSeparator.ToString(), "", rootFolder, customerUid, projectUid);
    }
  }
}
