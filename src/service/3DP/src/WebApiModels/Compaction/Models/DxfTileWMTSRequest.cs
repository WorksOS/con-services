using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// The request representation for getting a DXF tile from TCC
  /// </summary>
  public class DxfTileWMTSRequest : IValidatable, IDxfTileRequest
  {
    /// <summary>
    /// The files for which to retrieve the tiles.
    /// </summary>
    [Required]
    public IEnumerable<FileData> files { get; private set; }

    /// <summary>
    /// The x position the tile
    /// </summary>
    [Required]
    public int X { get; private set; }

    /// <summary>
    /// The y  of the tile
    /// </summary>
    [Required]
    public int Y { get; private set; }

    /// <summary>
    /// The zoom of the tile
    /// </summary>
    [Required]
    public int Z { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private DxfTileWMTSRequest()
    {
    }

    /// <summary>
    /// Create instance of CompactionTileRequest
    /// </summary>
    public static DxfTileWMTSRequest CreateTileRequest(
      IEnumerable<FileData> files,
      int x,
      int y,
      int z
    )
    {
      return new DxfTileWMTSRequest
      {
          files = files,
          X = x,
          Y = y,
          Z = z
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
