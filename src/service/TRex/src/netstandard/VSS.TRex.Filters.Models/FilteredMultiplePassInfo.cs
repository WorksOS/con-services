using System;
using Apache.Ignite.Core.Binary;
using VSS.ConfigurationStore;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.DI;
using VSS.TRex.Types;

namespace VSS.TRex.Filters.Models
{
  /// <summary>
  /// FilteredMultiplePassInfo records all the information that a filtering operation
  ///   selected from an IC grid cell containing all the recorded machine passes.
  /// </summary>
  public class FilteredMultiplePassInfo
  {
    /// <summary>
    /// PassCount keeps track of the actual number of passes in the list
    /// </summary>
    public int PassCount;

    /// <summary>
    /// The set of passes selected by the filtering operation
    /// </summary>
    public FilteredPassData[] FilteredPassData;

    private int CellPassAggregationListSizeIncrement() => DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT", Consts.VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT);

    /// <summary>
    /// Adds a pass to the set of passes beign constructed as a result of the filtering operation.
    /// </summary>
    /// <param name="pass"></param>
    /// <param name="passesOrderedInIncreasingTime"></param>
    public void AddPass(CellPass pass, bool passesOrderedInIncreasingTime = true)
    {
      /*TODO convert when C# equivalent of IFOPT C+ is understood
       {$IFOPT C+}
        if PassesOrderedInIncreasingTime then
          begin
            if (FPassCount > 0) and(FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.Time + OneSecond)) then
             Assert(False, Format('Passes not added to filtered pass list in increasing time order (1) (Time1 vs Time2 = %.6f (%s) vs %.6f (%s)',
                                   [FilteredPassData[FPassCount - 1].FilteredPass.Time,
                                    FormatCellPassTimeValue(FilteredPassData[FPassCount - 1].FilteredPass.Time, cpftWithMilliseconds, False),
                                     Pass.Time,
                                     FormatCellPassTimeValue(Pass.Time, cpftWithMilliseconds, False)])); {SKIP}
          end
        else
          begin
            Assert(((FPassCount = 0) or
                    (FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.Time - OneSecond))),
                   'Passes not added to filtered pass list in decreasing time order'); {SKIP}
          end;
      {$ENDIF}
      */

      // Increase the length of the passes array
      if (FilteredPassData == null)
      {
        FilteredPassData = new FilteredPassData[CellPassAggregationListSizeIncrement()];
      }
      else
      {
        if (PassCount == FilteredPassData.Length)
        {
          Array.Resize(ref FilteredPassData, PassCount + CellPassAggregationListSizeIncrement());
        }
      }

      // Add the pass to the list
      FilteredPassData[PassCount].FilteredPass = pass;

      PassCount++;
    }

    public void AddPass(FilteredPassData pass, bool passesOrderedInIncreasingTime)
    {
      /* TODO include when IFOPT C+ equivalent is identified
      {$IFOPT C+}
      if PassesOrderedInIncreasingTime then
        begin
        if (FPassCount > 0) and(FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.FilteredPass.Time + OneSecond)) then
         Assert(False, Format('Passes not added to filtered pass list in increasing time order (2) (Time1 vs Time2 = %.6f vs %.6f', [FilteredPassData[FPassCount - 1].FilteredPass.Time, Pass.FilteredPass.Time])); { SKIP}
      end
    else
      begin
        Assert(((FPassCount = 0) or
        (FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.FilteredPass.Time - OneSecond))),
       'Passes not added to filtered pass list in decreasing time order'); { SKIP}
      end;
      {$ENDIF}
      */

      if (FilteredPassData == null)
      {
        FilteredPassData = new FilteredPassData[CellPassAggregationListSizeIncrement()];
      }
      else // Increase the length of the passes array
      {
        if (PassCount == FilteredPassData.Length)
        {
          Array.Resize(ref FilteredPassData, PassCount + CellPassAggregationListSizeIncrement());
        }
      }

      // Add the pass to the list
      FilteredPassData[PassCount] = pass;

      PassCount++;
    }

    /// <summary>
    /// Assigns (copies) the set of filtered passes from another instance to this instance
    /// </summary>
    /// <param name="Source"></param>
    public void Assign(FilteredMultiplePassInfo Source)
    {
      if (PassCount < Source.PassCount)
        FilteredPassData = new FilteredPassData[PassCount];

      PassCount = Source.PassCount;

      Array.Copy(Source.FilteredPassData, FilteredPassData, PassCount);
    }

    /// <summary>
    /// Clear the set of filtered cell passes
    /// </summary>
    public void Clear()
    {
      PassCount = 0;
    }

    /// <summary>
    /// Returns the time of the first cell pass in the set of filtered cell passes
    /// </summary>
    public DateTime FirstPassTime => PassCount > 0 ? FilteredPassData[0].FilteredPass.Time : DateTime.MinValue;

    /// <summary>
    /// Determines the time of the cell pass with the highest elevation in the set of cell passes
    /// </summary>
    /// <returns></returns>
    public DateTime HighestPassTime()
    {
      float TempHeight = Consts.NullHeight;

      DateTime Result = DateTime.MinValue;

      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (TempHeight == Consts.NullHeight)
        {
          TempHeight = FilteredPassData[i].FilteredPass.Height;
          Result = FilteredPassData[i].FilteredPass.Time;
        }
        else
        {
          if (FilteredPassData[i].FilteredPass.Height > TempHeight)
          {
            TempHeight = FilteredPassData[i].FilteredPass.Height;
            Result = FilteredPassData[i].FilteredPass.Time;
          }
        }
      }

      return Result;
    }


    /// <summary>
    /// Determine the time of the last cell pass in the set of filtered cell passes
    /// </summary>
    /// <returns></returns>
    public DateTime LastPassTime() => PassCount > 0 ? FilteredPassData[PassCount - 1].FilteredPass.Time : DateTime.MinValue;


    public ushort LastPassValidAmp()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.Amplitude != CellPassConsts.NullAmplitude)
          return FilteredPassData[i].FilteredPass.Amplitude;

      return CellPassConsts.NullAmplitude;
    }

    public void LastPassValidCCVDetails(out short aCCV, out short aTarget)
    {
      aCCV = CellPassConsts.NullCCV;
      aTarget = CellPassConsts.NullCCV;
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (FilteredPassData[i].TargetValues.TargetCCV != CellPassConsts.NullCCV && aTarget == CellPassConsts.NullCCV) 
          aTarget = FilteredPassData[i].TargetValues.TargetCCV; // just in case ccv is missing but not target

        if (FilteredPassData[i].FilteredPass.CCV != CellPassConsts.NullCCV)
        {
          aCCV = FilteredPassData[i].FilteredPass.CCV;
          aTarget = FilteredPassData[i].TargetValues.TargetCCV; // update target with this record
          return;
        }
      }
    }

    public byte LastPassValidCCA()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.CCA != CellPassConsts.NullCCA)
          return FilteredPassData[i].FilteredPass.CCA;

      return CellPassConsts.NullCCA;
    }

    public void LastPassValidCCADetails(out byte aCCA, out byte aTarget)
    {
      aCCA = CellPassConsts.NullCCA;
      aTarget = CellPassConsts.NullCCA;
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (FilteredPassData[i].TargetValues.TargetCCA != CellPassConsts.NullCCA && aTarget == CellPassConsts.NullCCA)
          aTarget = FilteredPassData[i].TargetValues.TargetCCA; // just in case cca is missing but not target

        if (FilteredPassData[i].FilteredPass.CCA != CellPassConsts.NullCCA)
        {
          aCCA = FilteredPassData[i].FilteredPass.CCA;
          aTarget = FilteredPassData[i].TargetValues.TargetCCA; // update target with this record
          return;
        }
      }
    }

    public short LastPassValidCCV()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.CCV != CellPassConsts.NullCCV)
          return FilteredPassData[i].FilteredPass.CCV;

      return CellPassConsts.NullCCV;
    }

    public double LastPassValidCCVPercentage()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.CCV != CellPassConsts.NullCCV)
        {
          short CCVtarget = FilteredPassData[i].TargetValues.TargetCCV;
          if (CCVtarget != 0 && CCVtarget != CellPassConsts.NullCCV)
            return FilteredPassData[i].FilteredPass.CCV / CCVtarget;

          return CellPassConsts.NullCCVPercentage;
        }

      return CellPassConsts.NullCCVPercentage;
    }

    public ushort LastPassValidFreq()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.Frequency != CellPassConsts.NullFrequency)
          return FilteredPassData[i].FilteredPass.Frequency;

      return CellPassConsts.NullFrequency;
    }

    public short LastPassValidMDP()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.MDP != CellPassConsts.NullMDP)
          return FilteredPassData[i].FilteredPass.MDP;

      return CellPassConsts.NullMDP;
    }

    public void LastPassValidMDPDetails(out short aMDP, out short aTarget)
    {
      aMDP = CellPassConsts.NullMDP;
      aTarget = CellPassConsts.NullMDP;
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (FilteredPassData[i].TargetValues.TargetMDP != CellPassConsts.NullMDP && aTarget == CellPassConsts.NullMDP)
          aTarget = FilteredPassData[i].TargetValues.TargetMDP; // just in case ccv is missing but not target

        if (FilteredPassData[i].FilteredPass.MDP != CellPassConsts.NullMDP)
        {
          aMDP = FilteredPassData[i].FilteredPass.MDP;
          aTarget = FilteredPassData[i].TargetValues.TargetMDP; // update target with this record
          return;
        }
      }
    }
    public double LastPassValidMDPPercentage()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.MDP != CellPassConsts.NullMDP)
        {
          short MDPtarget = FilteredPassData[i].TargetValues.TargetCCV;
          if (MDPtarget != 0 && MDPtarget != CellPassConsts.NullMDP)
            return FilteredPassData[i].FilteredPass.MDP / MDPtarget;

          return CellPassConsts.NullMDPPercentage;
        }

      return CellPassConsts.NullMDPPercentage;
    }

    public GPSMode LastPassValidGPSMode()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.gpsMode != CellPassConsts.NullGPSMode)
          return FilteredPassData[i].FilteredPass.gpsMode;

      return CellPassConsts.NullGPSMode;
    }

    public byte LastPassValidRadioLatency()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.RadioLatency != CellPassConsts.NullRadioLatency)
          return FilteredPassData[i].FilteredPass.RadioLatency;

      return CellPassConsts.NullRadioLatency;
    }

    public short LastPassValidRMV()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.RMV != CellPassConsts.NullRMV)
          return FilteredPassData[i].FilteredPass.RMV;

      return CellPassConsts.NullRMV;
    }

    public DateTime LowestPassTime()
    {
      float TempHeight = Consts.NullHeight;
      DateTime Result = DateTime.MinValue;

      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (TempHeight == Consts.NullHeight)
        {
          TempHeight = FilteredPassData[i].FilteredPass.Height;
          Result = FilteredPassData[i].FilteredPass.Time;
        }
        else 
          if (FilteredPassData[i].FilteredPass.Height < TempHeight)
            Result = FilteredPassData[i].FilteredPass.Time;
      }

      return Result;
    }

    public ushort LastPassValidMaterialTemperature()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (FilteredPassData[i].FilteredPass.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
          return FilteredPassData[i].FilteredPass.MaterialTemperature;

      return CellPassConsts.NullMaterialTemperatureValue;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteInt(PassCount);

      writer.WriteBoolean(FilteredPassData != null);
      if (FilteredPassData != null)
      {
        writer.WriteInt(FilteredPassData.Length);
        foreach (var pass in FilteredPassData)
          pass.ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      PassCount = reader.ReadInt();

      if (reader.ReadBoolean())
      {
        var count = reader.ReadInt();
        FilteredPassData = new FilteredPassData[count];
        foreach (var pass in FilteredPassData)
          pass.FromBinary(reader);
      }
    }
  }
}
