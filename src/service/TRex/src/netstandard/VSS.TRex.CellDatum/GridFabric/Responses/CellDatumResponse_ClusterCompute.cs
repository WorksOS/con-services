using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common;

namespace VSS.TRex.CellDatum.GridFabric.Responses
{
  public class CellDatumResponse_ClusterCompute : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The internal result code resulting from the request.
    /// Values are: 0 = Value found, 1 = No value found, 2 = Unexpected error
    /// </summary>
    public CellDatumReturnCode ReturnCode { get; set; }

    /// <summary>
    /// The value from the request, scaled in accordance with the underlying attribute domain.
    /// </summary>
    public double? Value { get; set; }

    /// <summary>
    /// The date and time of the value.
    /// </summary>
    public DateTime? TimeStampUTC { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)ReturnCode);
      writer.WriteBoolean(Value.HasValue);
      if (Value.HasValue)
        writer.WriteDouble(Value.Value);
      writer.WriteBoolean(TimeStampUTC.HasValue);
      if (TimeStampUTC.HasValue)
        writer.WriteLong(TimeStampUTC.Value.ToBinary());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ReturnCode = (CellDatumReturnCode)reader.ReadInt();
      Value = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;
      TimeStampUTC = reader.ReadBoolean() ? DateTime.FromBinary(reader.ReadLong()) : (DateTime?)null;
    }

  }
}
