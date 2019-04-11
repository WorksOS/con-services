using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Rendering.Palettes
{
  public class SpeedSummaryPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const ushort SPEED_DEFAULT_RANGE_MIN = 10;
    private const ushort SPEED_DEFAULT_RANGE_MAX = 30;

    /// <summary>
    /// The color, which Machine Speed summary data displayed in on a plan view map, where machine speed values are greater than target range.
    /// </summary>
    public Color OverSpeedRangeColour = Color.Red;

    /// <summary>
    /// The color, which Machine Speed summary data displayed in on a plan view map, where machine speed values are within target range.
    /// </summary>
    public Color WithinSpeedRangeColour = Color.Lime;

    /// <summary>
    /// The color, which Machine Speed summary data displayed in on a plan view map, where machine speed values are less than target range.
    /// </summary>
    public Color LowerSpeedRangeColour = Color.Blue;

    /// <summary>
    /// Machine Speed target range.
    /// </summary>
    public MachineSpeedExtendedRecord MachineSpeedTarget = new MachineSpeedExtendedRecord(SPEED_DEFAULT_RANGE_MIN, SPEED_DEFAULT_RANGE_MAX);

    public SpeedSummaryPalette() : base(null)
    {
      // ...
    }

    public Color ChooseColour(MachineSpeedExtendedRecord measuredSpeed)
    {
      var color = Color.Empty;

      if (MachineSpeedTarget.Max != CellPassConsts.NullMachineSpeed)
      {
        if (measuredSpeed.Max > MachineSpeedTarget.Max)
          color = OverSpeedRangeColour;
        else 
          color = measuredSpeed.Min < MachineSpeedTarget.Min && measuredSpeed.Max < MachineSpeedTarget.Min 
            ? LowerSpeedRangeColour
            : WithinSpeedRangeColour;
      }

      return color;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(OverSpeedRangeColour.ToArgb());
      writer.WriteInt(WithinSpeedRangeColour.ToArgb());
      writer.WriteInt(LowerSpeedRangeColour.ToArgb());

      MachineSpeedTarget.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OverSpeedRangeColour = Color.FromArgb(reader.ReadInt());
      WithinSpeedRangeColour = Color.FromArgb(reader.ReadInt());
      LowerSpeedRangeColour = Color.FromArgb(reader.ReadInt());

      MachineSpeedTarget.FromBinary(reader);
    }
  }
}
