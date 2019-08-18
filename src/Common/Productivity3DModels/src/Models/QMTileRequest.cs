using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Request to get a quantized mesh tile
  /// </summary>
  public class QMTileRequest : TRexBaseRequest
  {

    [JsonProperty(Required = Required.Always)]
    public int X { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Y { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Z { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int DisplayMode{ get; set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public QMTileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters. x,y,z are tile coordinates.
    /// </summary>
    /// <param name="filter">Filter 1</param>
    public QMTileRequest(
      Guid projectUid,
      FilterResult filter,
      int displayMode,
      int x,
      int y,
      int z)
    {
      ProjectUid = projectUid;
      Filter = filter;
      DisplayMode = displayMode;
      X = x;
      Y = y;
      Z = z;
    }
  }
}
