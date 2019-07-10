using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  public class QMTileRequest : RaptorHelper
  {

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId1", Required = Required.Default)]
    public long FilterId1 { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int X { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Y { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Z { get; set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public QMTileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUId">Project ID</param>
    /// <param name="callId">Caller ID</param>
    /// <param name="filter1">Filter 1</param>
    /// <param name="filterId1">Fileter ID</param>
    /// <param name="X">tile X coordinate</param>
    /// <param name="Y">tile Y coordinate</param>
    /// <param name="Z">tile Z coordinate</param>
    public QMTileRequest(
      Guid? projectUid,
      Guid? callId,
      FilterResult filter1,
      long filterId1,
      int x,
      int y,
      int z)
    {
      ProjectUid = projectUid;
      CallId = callId;
      Filter1 = filter1;
      FilterId1 = filterId1;
      X = x;
      Y = y;
      Z = z;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      Filter1?.Validate();
    }

  }
}
