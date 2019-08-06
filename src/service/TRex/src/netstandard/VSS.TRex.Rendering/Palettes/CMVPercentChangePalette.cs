using System;
using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Palettes
{
  public class CMVPercentChangePalette : CMVPalette
  {
    private const byte VERSION_NUMBER = 1;

    private const double MAX_PERCENTAGE_VALUE = 100.0;

    public bool UseAbsoluteValues { get; set; }

    private static Transition[] Transitions =
    {
      new Transition(short.MinValue, Color.FromArgb(213, 0, 0)),
      new Transition(-50, Color.FromArgb(229, 115, 115)),
      new Transition(-20, Color.FromArgb(255, 205, 210)),
      new Transition(-10, Color.FromArgb(139, 195, 74)),
      new Transition(0, Color.FromArgb(179, 229, 252)),
      new Transition(10, Color.FromArgb(79, 195, 247)),
      new Transition(20, Color.FromArgb(3, 155, 229)),
      new Transition(50, Color.FromArgb(1, 87, 155))
    };

    public CMVPercentChangePalette() : base(Transitions)
    {
    }

    public Color ChooseColour(SubGridCellPassDataCMVEntryRecord cmvData)
    {
      if (CMVCellValueToDisplay(cmvData, out var cmvPercentValue))
      {
        if (cmvData.IsDecoupled && DisplayDecoupledColourInPVM)
          return DefaultDecoupledCMVColour;

        // Check to see if the value is in the target range and use the target CMV colour
        // if it is. CCVRange holds a min/max percentage of target CMV...
        if (DisplayTargetCCVColourInPVM && Range.InRange(cmvPercentValue, MAX_PERCENTAGE_VALUE * _minTarget, MAX_PERCENTAGE_VALUE * _maxTarget))
          return TargetCCVColour;

        return ChooseColour(cmvPercentValue);
      }

      return Color.Empty;
    }

    private bool CMVCellValueToDisplay(SubGridCellPassDataCMVEntryRecord cmvData, out double cmvPercentValue)
    {
      cmvPercentValue = 0.0;

      if (cmvData.MeasuredCMV == CellPassConsts.NullCCV)
        return false;

      if (cmvData.PreviousMeasuredCMV == CellPassConsts.NullCCV)
      {
        cmvPercentValue = MAX_PERCENTAGE_VALUE;
        return true;
      }

      if (UseAbsoluteValues)
      {
        var tempValue = Math.Abs(cmvData.MeasuredCMV - cmvData.PreviousMeasuredCMV) / (double) cmvData.PreviousMeasuredCMV;

        cmvPercentValue = tempValue * MAX_PERCENTAGE_VALUE;
        return true;
      }

      double previousPercentage;
      double currentPercentage;

      if (UseMachineTargetCMV)
      {
        if (cmvData.TargetCMV == CellPassConsts.NullCCV || cmvData.PreviousTargetCMV == CellPassConsts.NullCCV)
          return false;

        if (cmvData.TargetCMV != 0 && cmvData.PreviousTargetCMV != 0)
        {
          double tempValue = (double) cmvData.PreviousTargetCMV / CellPassConsts.CCVvalueRatio;
          previousPercentage = cmvData.PreviousMeasuredCMV / tempValue * MAX_PERCENTAGE_VALUE;

          tempValue = (double)cmvData.TargetCMV / CellPassConsts.CCVvalueRatio;
          currentPercentage = cmvData.MeasuredCMV / tempValue * MAX_PERCENTAGE_VALUE;
        }
        else
          return false;
      }
      else
      {
        if (AbsoluteTargetCMV != 0)
        {
          double tempValue = (double) AbsoluteTargetCMV / CellPassConsts.CCVvalueRatio;

          previousPercentage = cmvData.PreviousMeasuredCMV / tempValue * MAX_PERCENTAGE_VALUE;
          currentPercentage = cmvData.MeasuredCMV / tempValue * MAX_PERCENTAGE_VALUE;
        }
        else
          return false;
      }

      cmvPercentValue = (currentPercentage - previousPercentage) / previousPercentage * MAX_PERCENTAGE_VALUE;
      return true;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(UseAbsoluteValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      UseAbsoluteValues = reader.ReadBoolean();
    }
  }
}
