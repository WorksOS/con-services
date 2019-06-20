using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Tile.Service.Common.Models
{
  /// <summary>
  /// The request representation for getting a DXF tile from TCC
  /// </summary>
  public class DxfTile3dRequest 
  {
    /// <summary>
    /// The files for which to retrieve the tiles.
    /// </summary>
    [Required]
    public IEnumerable<FileData> files { get; private set; }

    public int zoomLevel { get; private set; }
    public int yTile { get; private set; }
    public int xTile { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private DxfTile3dRequest()
    {
    }

    /// <summary>
    /// Create instance of DxfTile3dRequest
    /// </summary>
    public static DxfTile3dRequest Create(
      IEnumerable<FileData> files,
      int zoom,
      int y,
      int x
    )
    {
      return new DxfTile3dRequest
      {
        files = files,
        zoomLevel = zoom,
        yTile = y,
        xTile = x
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
    }
  }
}
