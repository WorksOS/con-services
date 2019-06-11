using System;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface ISubGridSpatialAffinityKey
  {
    /// <summary>
    /// The version number of this spatial element when it is stored in the persistent layer, defined
    /// as the number of ticks in DateTime.UtcNow at the time it is written.
    /// </summary>
    long Version { get; set; }

    /// <summary>
    /// The GUID for the project the sub grid data belongs to.
    /// </summary>
    Guid ProjectUID { get; set; }

    /// <summary>
    /// The X ordinate cell address of the origin cell for the sub grid
    /// </summary>
    int SubGridX { get; set; }

    /// <summary>
    /// The Y ordinate cell address of the origin cell for the sub grid
    /// </summary>
    int SubGridY { get; set; }

    /// <summary>
    /// The start date for the data held if this key refers to a segment. It will be -1 if it does not.
    /// </summary>
    long SegmentStartDate { get; set; } // in ticks

    /// <summary>
    /// The end date for the data held if this key refers to a segment. It will be -1 if it does not.
    /// </summary>
    long SegmentEndDate { get; set; } // in ticks

    /// <summary>
    /// Converts the spatial segment affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    /// <returns></returns>
    string ToString();
  }
}
