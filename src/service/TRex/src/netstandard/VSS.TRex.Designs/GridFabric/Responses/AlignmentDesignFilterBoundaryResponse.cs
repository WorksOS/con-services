using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  /// <summary>
  /// Represents the response to a request for a polygonal boundary comprising grid coordinate vertices that represents
  /// the boundary of an area of an alignment design described by its starting and ending station plus left and right
  /// offset values
  /// </summary>
  public class AlignmentDesignFilterBoundaryResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public Fence Boundary { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Boundary != null);
      Boundary?.ToBinary(writer);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (reader.ReadBoolean())
        (Boundary ?? (Boundary = new Fence())).FromBinary(reader);
    }
  }
}
