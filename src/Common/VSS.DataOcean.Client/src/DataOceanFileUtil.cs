using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.DataOcean.Client
{
  public class DataOceanFileUtil
  {
    public const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    public const string GENERATED_ALIGNMENT_CENTERLINE_FILE_SUFFIX = "_AlignmentCenterline$";
    public const string DXF_FILE_EXTENSION = ".dxf";
    public string[] SUPPORTED_FILE_EXTENSIONS = {".tif",".tiff", DXF_FILE_EXTENSION};

    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public string FullFileName { get; private set; }
    public string TilesPath => TilePath();
    public string TilesMetadataFileName => TileMetadataFileName();
    public string GeneratedTilesFolder => TilesFolderWithSuffix();

    public DataOceanFileUtil(string fileName, string path)
    {
      FileName = fileName;
      FilePath = path;
      FullFileName = $"{path}{Path.DirectorySeparatorChar}{fileName}";
      var extension = Path.GetExtension(fileName);
      if (!SUPPORTED_FILE_EXTENSIONS.Contains(extension, StringComparer.OrdinalIgnoreCase)) 
      {
        throw new ArgumentException($"Only DXF and GeoTIFF files are supported. {fileName} is not a DXF or GeoTIFF file.");
      }
    }

    public DataOceanFileUtil(string fullFileName) : this(Path.GetFileName(fullFileName), Path.GetDirectoryName(fullFileName))
    {
    }

    /// <summary>
    /// The generated folder name for DXF tiles to be stored in
    /// </summary>
    /// <returns>The tile folder name</returns>
    private string TilesFolderWithSuffix()
    {
      return $"{FilePath}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(FileName)}{GENERATED_TILE_FOLDER_SUFFIX}";
    }

    /// <summary>
    /// The top level folder of all tile files
    /// </summary>
    /// <returns></returns>
    private string BaseTilePath()
    {
      return $"{TilesFolderWithSuffix()}/tiles";
    }

    /// <summary>
    /// The full folder name of where the tiles are stored
    /// </summary>
    /// <returns>The full path of the tile folder</returns>
    private string TilePath()
    {
      return $"{BaseTilePath()}/xyz";
    }


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
    /// <param name="zoomLevel">The zoom level.</param>
    /// <param name="topLeftTileY">The top left tile y.</param>
    /// <param name="topLeftTileX">The top left tile x.</param>
    /// <returns></returns>
    public string GetTileFileName(int zoomLevel, int topLeftTileY, int topLeftTileX)
    {
      return $"{ZoomPath(zoomLevel)}/{topLeftTileY}/{topLeftTileX}.png";
    }

    /// <summary>
    /// The path to the zoom folder name for the specified zoom level
    /// </summary>
    /// <param name="zoomLevel">The zoom level</param>
    /// <returns>The zoom path</returns>
    private string ZoomPath(int zoomLevel)
    {
      return $"{TilePath()}/{zoomLevel}";
    }

    /// <summary>
    /// Extracts the full name of the filename from tile filename.
    /// </summary>
    /// <param name="fullTileName">The full path and name of the tile file.</param>
    /// <returns>The base file name to which the tiles belong</returns>
    /// <exception cref="System.ArgumentException"></exception>
    public static string ExtractBaseFileNameFromTileFullName(string fullTileName)
    {
      return ExtractNameFromTileFullName(fullTileName, 0);
    }

    /// <summary>
    /// Extracts the multipart path and file name of the tile. This is the path from the generated folder name
    /// down to the actual tile file name.
    /// </summary>
    /// <param name="fullTileName"></param>
    /// <returns></returns>
    public static string ExtractTileNameFromTileFullName(string fullTileName)
    {
      return ExtractNameFromTileFullName(fullTileName, 1);
    }

    /// <summary>
    /// Extracts either the base path or multipart path for the tile
    /// </summary>
    /// <param name="fullTileName"></param>
    /// <param name="indx"></param>
    /// <returns></returns>
    private static string ExtractNameFromTileFullName(string fullTileName, int indx)
    {
      if (!fullTileName.Contains(GENERATED_TILE_FOLDER_SUFFIX))
        throw new ArgumentException($"Invalid fullname - no expected suffix {fullTileName}");
      var filename = fullTileName.Split(new string[] { GENERATED_TILE_FOLDER_SUFFIX }, StringSplitOptions.None)[indx];
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

    /// <summary>
    /// Gets the generated DXF file name
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
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
    /// Construct the path in DataOcean
    /// </summary>
    public static string DataOceanPath(string rootFolder, string customerUid, string projectUid)
    {
      return $"{Path.DirectorySeparatorChar}{rootFolder}{Path.DirectorySeparatorChar}{customerUid}{Path.DirectorySeparatorChar}{projectUid}";
    }
  }
}
