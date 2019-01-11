using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;

using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  public class SummaryVolumeProfileCell : ProfileCellBase, ISummaryVolumeProfileCell
  {

    private static ILogger Log = Logging.Logger.CreateLogger<ProfileCell>();

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
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteFloat(LastCellPassElevation1);
      writer.WriteFloat(LastCellPassElevation2);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      LastCellPassElevation1 = reader.ReadFloat();
      LastCellPassElevation2 = reader.ReadFloat();
    }

  }
}
