using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  public class SummaryVolumeProfileCell : ProfileCellBase, ISummaryVolumeProfileCell
  {
    private const byte VERSION_NUMBER = 1;

    public float LastCellPassElevation1;
    public float LastCellPassElevation2;

    public SummaryVolumeProfileCell()
    {
      LastCellPassElevation1 = Consts.NullHeight;
      LastCellPassElevation2 = Consts.NullHeight;
      DesignElev = Consts.NullHeight;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteFloat(LastCellPassElevation1);
      writer.WriteFloat(LastCellPassElevation2);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        LastCellPassElevation1 = reader.ReadFloat();
        LastCellPassElevation2 = reader.ReadFloat();
      }
    }

    /// <summary>
    /// Summary volume cells can be calculated be comparison of a cell pass elevation and a design elevation,
    /// or by comparison of two cell pass elevations together.
    /// </summary>
    public override bool IsNull() => DesignElev == Consts.NullHeight && LastCellPassElevation1 == Consts.NullHeight && LastCellPassElevation2 == Consts.NullHeight;
  }
}
