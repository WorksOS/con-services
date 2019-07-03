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
  /// Argument containing the parameters required for a Cell Passes request
  /// </summary>    
  public class CellPassesRequestArgument_ApplicationService : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// A flag to indicate if a latitude/longitude or projected coordinate point has been provided
    /// </summary>
    public bool CoordsAreGrid { get; set; }

    /// <summary>
    /// The WGS84 latitude/longitude position or grid point in the project coordinate system to identify the cell from. 
    /// </summary>
    public XYZ Point { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

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

      CoordsAreGrid = reader.ReadBoolean(); 
      Point = Point.FromBinary(reader);   
    }

    public CellPassesRequestArgument_ApplicationService()
    {
      
    }

    public CellPassesRequestArgument_ApplicationService(Guid siteModelId,
      bool coordsAreGrid,
      XYZ point,
      IFilterSet filters)
    {
      ProjectID = siteModelId;
      CoordsAreGrid = coordsAreGrid;
      Point = point;
      Filters = filters;
    }

  }
}
