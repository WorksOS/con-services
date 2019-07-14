using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.QuantizedMesh.GridFabric.Arguments
{
  public class QuantizedMeshRequestArgument : BaseApplicationServiceRequestArgument
  {

    private const byte VERSION_NUMBER = 1;

    public bool CoordsAreGrid { get; set; }

    public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

    public QuantizedMeshRequestArgument()
    { }

    public QuantizedMeshRequestArgument(Guid siteModelID,
                                     BoundingWorldExtent3D extents,
                                     IFilterSet filters)
    {
      ProjectID = siteModelID;
      Extents = extents;
      CoordsAreGrid = false;
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

      writer.WriteBoolean(Extents != null);
      Extents.ToBinary(writer);

    }


    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (reader.ReadBoolean())
      {
        Extents = new BoundingWorldExtent3D();
        Extents.FromBinary(reader);
      }

      CoordsAreGrid = false;
    }

  }
}
