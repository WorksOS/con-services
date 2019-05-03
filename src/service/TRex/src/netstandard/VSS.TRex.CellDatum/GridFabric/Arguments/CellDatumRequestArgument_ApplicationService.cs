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
  /// <summary>
  /// Argument containing the parameters required for a Cell Datum request
  /// </summary>    
  public class CellDatumRequestArgument_ApplicationService : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The datum type to return (eg: height, CMV, Temperature etc). 
    /// </summary>
    public DisplayMode Mode { get; set; } = DisplayMode.Height;

    /// <summary>
    /// A flag to indicate if a latitude/longitude or projected coordinate point has been provided
    /// </summary>
    public bool CoordsAreGrid { get; set; }

    /// <summary>
    /// The WGS84 latitude/longitude position or grid point in the project coordinate system to identify the cell from. 
    /// </summary>
    public XYZ Point { get; set; }

    public CellDatumRequestArgument_ApplicationService()
    { }

    public CellDatumRequestArgument_ApplicationService(
      Guid siteModelID,
      DisplayMode mode,
      bool coordsAreGrid,
      XYZ point,
      IFilterSet filters,
      Guid referenceDesignUid,
      double referenceOffset)
    {
      ProjectID = siteModelID;
      Mode = mode;
      CoordsAreGrid = coordsAreGrid;
      Point = point;
      Filters = filters;
      ReferenceDesign.DesignID = referenceDesignUid;
      ReferenceDesign.Offset = referenceOffset;
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
      writer.WriteBoolean(CoordsAreGrid);
      Point.ToBinary(writer);
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
      CoordsAreGrid = reader.ReadBoolean(); 
      Point = Point.FromBinary(reader);   
    }
  }
}
