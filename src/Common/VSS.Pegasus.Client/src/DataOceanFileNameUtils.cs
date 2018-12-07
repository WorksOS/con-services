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
    public string TilesPath => TilePath(FilePath, FileName);

    public DataOceanFile(string fileName, string path)
    {
      FileName = fileName;
      FilePath = path;
      if (Path.GetExtension(fileName).ToUpper() != DXF_FILE_EXTENSION)
      {
        throw new ArgumentException($"Only DXF files are supported. {fileName} is not a DXF file.");
      }
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
      return $"{ZoomPath(TilePath(FilePath, FileName), zoomLevel)}/{topLeftTileY}/{topLeftTileX}.png";
    }

    /// <summary>
    /// The generated folder name for DXF tiles to be stored in
    /// </summary>
    /// <param name="dxfFileName">THe DXF file to which the tiles belong</param>
    /// <returns>The tile folder name</returns>
    private string TilesFolderWithSuffix(string dxfFileName)
    {
      return $"{dxfFileName}{GENERATED_TILE_FOLDER_SUFFIX}";
    }

    /// <summary>
    /// The full folder name of where the tiles are stored
    /// </summary>
    /// <param name="path">The full path of where the DXF file is located</param>
    /// <param name="generatedName">The DXF file name which is generated for an alignment or design file</param>
    /// <returns>The full path of the tile folder</returns>
    private string TilePath(string path, string generatedName)
    {
      return $"{path}/{TilesFolderWithSuffix(generatedName)}/tiles/xyz";
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
      return $"{tilePath}/{zoomLevel}";
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
