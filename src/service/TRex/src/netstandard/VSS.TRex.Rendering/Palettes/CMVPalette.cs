﻿using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw CMV data
  /// </summary>
  public class CMVPalette : CMVBasePalette
  {
    private const byte VERSION_NUMBER = 1;

    public bool DisplayTargetCCVColourInPVM { get; set; }
    public bool DisplayDecoupledColourInPVM { get; set; }
    
    public Color TargetCCVColour = Color.Blue;

    /// <summary>
    /// The default colour that is used to display decoupled CMV data.
    /// </summary>
    public Color DefaultDecoupledCMVColour = Color.Black;

    private static Transition[] Transitions =
    {
      new Transition(0, Color.Green),
      new Transition(20, Color.Yellow),
      new Transition(40, Color.Olive),
      new Transition(60, Color.Blue),
      new Transition(100, Color.SkyBlue)
    };

    public CMVPalette() : base(Transitions)
    {
    }

    public CMVPalette(Transition[] transitions) : base(transitions)
    {
    }

    public Color ChooseColour(SubGridCellPassDataCMVEntryRecord cmvData)
    {
      if (cmvData.IsDecoupled && DisplayDecoupledColourInPVM)
        return DefaultDecoupledCMVColour;

      // Check to see if the value is in the target range and use the target CMV colour
      // if it is. CCVRange holds a min/max percentage of target CMV...
      var targetCMVValue = !UseMachineTargetCMV ? AbsoluteTargetCMV : cmvData.TargetCMV;

      if (DisplayTargetCCVColourInPVM && Range.InRange(cmvData.MeasuredCMV, targetCMVValue * _minTarget, targetCMVValue * _maxTarget))
        return TargetCCVColour;

      return ChooseColour(cmvData.MeasuredCMV);
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(DisplayDecoupledColourInPVM);
      writer.WriteBoolean(DisplayTargetCCVColourInPVM);
      writer.WriteInt(TargetCCVColour.ToArgb());
      writer.WriteInt(DefaultDecoupledCMVColour.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        DisplayDecoupledColourInPVM = reader.ReadBoolean();
        DisplayTargetCCVColourInPVM = reader.ReadBoolean();
        TargetCCVColour = Color.FromArgb(reader.ReadInt());
        DefaultDecoupledCMVColour = Color.FromArgb(reader.ReadInt());
      }
    }
  }
}
