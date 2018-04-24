
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionTileRequest
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    /// 
    public long? projectId { get; set; }

    /// <summary>
    /// A project unique identifier.
    /// </summary>
    public string projectUid { get; set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    public DisplayMode mode { get; set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// In case of cut/fill data rendering the transition order should be datum value descendent.
    /// </summary>
    public List<ColorPalette> palette { get; set; }

    /// <summary>
    /// The filter to be used.
    /// </summary>
    public CompactionFilter filter { get; set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
    /// </summary>
    public BoundingBox2DLatLon boundBoxLL { get; set; }

    /// <summary>
    /// The width, in pixels, of the image tile to be rendered
    /// </summary>
    public ushort width { get; set; }

    /// <summary>
    /// The height, in pixels, of the image tile to be rendered
    /// </summary>
    public ushort height { get; set; }
  }
}
