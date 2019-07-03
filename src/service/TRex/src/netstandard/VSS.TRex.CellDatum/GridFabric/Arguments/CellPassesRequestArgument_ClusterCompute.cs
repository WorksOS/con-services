using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.CellDatum.GridFabric.Arguments
{
  public class CellPassesRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The grid point in the project coordinate system to identify the cell from. 
    /// </summary>
    public XYZ NEECoords { get; set; }

    /// <summary>
    /// On the ground coordinates for the cell
    /// </summary>
    public int OTGCellX { get; set; }
    public int OTGCellY { get; set; }

    public CellPassesRequestArgument_ClusterCompute()
    {
      
    }

    public CellPassesRequestArgument_ClusterCompute(Guid siteModelID,
      XYZ neeCoords,
      int otgCellX,
      int otgCellY,
      IFilterSet filters)
    {
      ProjectID = siteModelID;
      NEECoords = neeCoords;
      OTGCellX = otgCellX;
      OTGCellY = otgCellY;
      Filters = filters;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      NEECoords.ToBinary(writer);
      writer.WriteInt(OTGCellX);
      writer.WriteInt(OTGCellY);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      NEECoords = NEECoords.FromBinary(reader);
      OTGCellX = reader.ReadInt();
      OTGCellY = reader.ReadInt();
    }
  }
}
