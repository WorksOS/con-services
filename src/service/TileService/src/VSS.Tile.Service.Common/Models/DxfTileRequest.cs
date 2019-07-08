using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Tile.Service.Common.Models
{
  /// <summary>
  /// The request representation for getting a DXF tile from DataOcean
  /// </summary>
  public class DxfTileRequest 
  {
    /// <summary>
    /// The files for which to retrieve the tiles.
    /// </summary>
    [Required]
    public IEnumerable<FileData> files { get; private set; }

    /// <summary>
    /// The bounding box of the tile
    /// </summary>
    [Required]
    public BoundingBox2DLatLon bbox { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private DxfTileRequest()
    {
    }

    /// <summary>
    /// Create instance of DxfTileRequest
    /// </summary>
    public static DxfTileRequest CreateTileRequest(
      IEnumerable<FileData> files,
      BoundingBox2DLatLon boundingBoxLatLon
    )
    {
      return new DxfTileRequest
      {
        files = files,
        bbox = boundingBoxLatLon
      };
    }

  }
}
