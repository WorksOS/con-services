using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The representation of a Patch request. A patch defines a series of subgrids of cell data returned to the caller. patchNumber and patchSize control which patch of
  /// subgrid and cell data need to be returned within the overall set of patches that comprise the overall data set identified by the thematic dataset, filtering and
  /// analytics parameters within the request.
  /// Requesting patch number 0 will additionally return a summation of the total number of patches of the requested size that need to be requested in order to assemble the
  /// complete data set.
  /// </summary>
  public class PatchRequest : RequestBase
  {
    /// <summary>
    /// Project ID. Required.
    /// </summary>
    public long? projectId { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    public DisplayMode mode { get; set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// </summary>
    public List<ColorPalette> palettes { get; set; }

    /// <summary>
    /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// Render the thematic data into colours using the supplied color palettes.
    /// </summary>
    public bool renderColorValues { get; set; }

    /// <summary>
    /// The volume computation type to use for summary volume thematic rendering
    /// </summary>
    public VolumesType computeVolType { get; set; }

    /// <summary>
    /// The tolerance to be used to indicate no change in volume for a cell. Used for summary volume thematic rendering. Value is expressed in meters.
    /// </summary>
    public double computeVolNoChangeTolerance { get; set; }

    /// <summary>
    /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
    /// </summary>
    public DesignDescriptor designDescriptor { get; set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    public FilterResult filter1 { get; set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    public long filterId1 { get; set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    public FilterResult filter2 { get; set; }

    /// <summary>
    /// The ID of the top or latest filter to be used.
    /// </summary>
    public long filterId2 { get; set; }

    /// <summary>
    /// The method of filtering cell passes into layers to be used for thematic renderings that require layer analysis as an input into the rendered data.
    /// If this value is provided any layer method provided in a filter is ignored.
    /// </summary>
    public FilterLayerMethod filterLayerMethod { get; set; }

    /// <summary>
    /// The number of the patch of data to be requested in the overall series of patches covering the required dataset.
    /// </summary>
    public int patchNumber { get; set; }

    /// <summary>
    /// The number of subgrids to return in the patch
    /// </summary>
    public int patchSize { get; set; }
  }
}
