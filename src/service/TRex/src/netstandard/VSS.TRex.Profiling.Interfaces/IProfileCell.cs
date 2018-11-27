using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Cells;

namespace VSS.TRex.Profiling.Interfaces
{
  /// <summary>
  /// Currently just a generic interface holder for a profile cell. All consumers will need to cast to a ProfileCell concrete implementation to access it
  /// </summary>
  public interface IProfileCell: IEquatable<IProfileCell>
  {
    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    uint OTGCellX { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    uint OTGCellY { get; set; }

    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    double Station { get; set; }

    ushort CellMaxSpeed { get; set; }
    ushort CellMinSpeed { get; set; }

    /// <summary>
    /// A collection of layers constituting a profile through a cell.
    /// Depending on the context, the layers may be equivalent to the passes over a cell
    /// or may represent the lifts over a cell, in which case the Passes collection
    /// for an individual layer will contain the passes making up that lift.
    /// </summary>
    IProfileLayers Layers { get; set; }

    /// <summary>
    /// Determines if the recorded time of a given pass lies within the time range of a layer that is
    /// deemed to be superceded by another layer
    /// </summary>
    /// <param name="Pass"></param>
    /// <returns></returns>
    bool IsInSupersededLayer(CellPass Pass);

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    void ToBinary(IBinaryRawWriter writer);

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    void FromBinary(IBinaryRawReader reader);
  }
}
