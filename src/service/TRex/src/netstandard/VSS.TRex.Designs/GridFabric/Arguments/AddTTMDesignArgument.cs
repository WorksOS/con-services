using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  /// <summary>
  /// Contains the parameters for addition and modification of designs in a project
  /// </summary>
  public class AddTTMDesignArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The descriptor of the design being added or modified
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; set; }

    /// <summary>
    /// The boundaing rectangle conputed for the design
    /// </summary>
    public BoundingWorldExtent3D Extents { get; set; }
      
    /// <summary>
    /// The spatial sub grid existence map for the area coveed by the design
    /// </summary>
    public ISubGridTreeBitMask ExistenceMap { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      DesignDescriptor.ToBinary(writer);
      Extents.ToBinary(writer);
      
      var bytes = ExistenceMap?.ToBytes();
      var haveBytes = bytes?.Length > 0;

      writer.WriteBoolean(haveBytes);

      if (haveBytes)
        writer.WriteByteArray(bytes);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      DesignDescriptor.FromBinary(reader);

      (Extents = new BoundingWorldExtent3D()).FromBinary(reader);

      var bytes = reader.ReadBoolean() ? reader.ReadByteArray() : null;
    
      if (bytes != null)
      {
        (ExistenceMap = new SubGridTreeSubGridExistenceBitMask()).FromBytes(bytes);
      }
    }
  }
}
