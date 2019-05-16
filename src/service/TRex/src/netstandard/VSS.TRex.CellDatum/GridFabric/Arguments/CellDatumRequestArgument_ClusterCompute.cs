using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.CellDatum.GridFabric.Arguments
{
  /// <summary>
  /// Argument containing the parameters required for a Cell Datum request
  /// </summary>    
  public class CellDatumRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The datum type to return (eg: height, CMV, Temperature etc). 
    /// </summary>
    public DisplayMode Mode { get; set; } = DisplayMode.Height;

    /// <summary>
    /// The grid point in the project coordinate system to identify the cell from. 
    /// </summary>
    public XYZ NEECoords { get; set; }

    /// <summary>
    /// On the ground coordinates for the cell
    /// </summary>
    public uint OTGCellX { get; set; }
    public uint OTGCellY { get; set; }

    public CellDatumRequestArgument_ClusterCompute()
    { }

    public CellDatumRequestArgument_ClusterCompute(
      Guid siteModelID,
      DisplayMode mode,
      XYZ neeCoords,
      uint otgCellX,
      uint otgCellY,
      IFilterSet filters,
      DesignOffset referenceDesign)
    {
      ProjectID = siteModelID;
      Mode = mode;
      NEECoords = neeCoords;
      OTGCellX = otgCellX;
      OTGCellY = otgCellY;
      Filters = filters;
      ReferenceDesign = referenceDesign;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)Mode);
      NEECoords.ToBinary(writer);
      writer.WriteInt((int)OTGCellX);
      writer.WriteInt((int)OTGCellY);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Mode = (DisplayMode)reader.ReadInt();
      NEECoords = NEECoords.FromBinary(reader);
      OTGCellX = (uint)reader.ReadInt();
      OTGCellY = (uint)reader.ReadInt();
    }
  }
}
