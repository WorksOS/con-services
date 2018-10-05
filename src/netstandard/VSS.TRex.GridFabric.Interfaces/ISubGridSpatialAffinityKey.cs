using System;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface ISubGridSpatialAffinityKey
  {
    /// <summary>
    /// The GUID for the project the subgrid data belongs to.
    /// </summary>
    Guid ProjectUID { get; set; }

    /// <summary>
    /// The X ordinate cell address of the origin cell for the subgrid
    /// </summary>
    uint SubGridX { get; set; }

    /// <summary>
    /// The Y ordinate cell address of the origin cell for the subgrid
    /// </summary>
    uint SubGridY { get; set; }

    /// <summary>
    /// The segment identifier for the subgrid data. If the segment identifier is empty then the element represents
    /// the subgrid directory (or SGL file). Otherwise, the segment identifier is a string representation of the start
    /// and end times of the segment and the time duration the segment contains data for.
    /// </summary>
    string SegmentIdentifier { get; set; }

    /// <summary>
    /// Converts the spatial segment affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    /// <returns></returns>
    string ToString();
  }
}
