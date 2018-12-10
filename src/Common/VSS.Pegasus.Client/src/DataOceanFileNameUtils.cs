using System;
using System.IO;

namespace VSS.Pegasus.Client
{
  //TODO: Move this to DataOcean Client
  public class DataOceanFile
  {
    private const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    public const string DXF_FILE_EXTENSION = ".DXF";

    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public string FullFileName { get; private set; }
    public string TilesPath => TilePath();
    public string TilesMetadataFileName => TileMetadataFileName();
    public string GeneratedTilesFolder => TilesFolderWithSuffix();

    public DataOceanFile(string fileName, string path)
    {
      FileName = fileName;
      FilePath = path;
      FullFileName = $"{path}{Path.DirectorySeparatorChar}{fileName}";
      if (Path.GetExtension(fileName).ToUpper() != DXF_FILE_EXTENSION)
      {
        throw new ArgumentException($"Only DXF files are supported. {fileName} is not a DXF file.");
      }
    }

    public DataOceanFile(string fullFileName) : this(Path.GetFileName(fullFileName), Path.GetDirectoryName(fullFileName))
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
      return $"{TilesFolderWithSuffix()}{Path.DirectorySeparatorChar}tiles";
    }

    /// <summary>
    /// The full folder name of where the tiles are stored
    /// </summary>
    /// <returns>The full path of the tile folder</returns>
    private string TilePath()
    {
      return $"{BaseTilePath()}{Path.DirectorySeparatorChar}xyz";
    }


    /// <summary>
    /// The full name of the tiles metadata file
    /// </summary>
    /// <returns>The full name of the tile metadata file</returns>
    public string TileMetadataFileName()
    {
      return $"{BaseTilePath()}{Path.DirectorySeparatorChar}tiles.json";
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
      return $"{ZoomPath(zoomLevel)}{Path.DirectorySeparatorChar}{topLeftTileY}{Path.DirectorySeparatorChar}{topLeftTileX}.png";
    }

    /// <summary>
    /// The path to the zoom folder name for the specified zoom level
    /// </summary>
    /// <param name="zoomLevel">The zoom level</param>
    /// <returns>The zoom path</returns>
    private string ZoomPath(int zoomLevel)
    {
      return $"{TilePath()}{Path.DirectorySeparatorChar}{zoomLevel}";
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
      var filename = fullTileName.Split(new string[] { GENERATED_TILE_FOLDER_SUFFIX }, StringSplitOptions.None)[0];
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
}
