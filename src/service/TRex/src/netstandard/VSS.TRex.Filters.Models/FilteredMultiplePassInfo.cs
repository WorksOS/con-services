using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.ConfigurationStore;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.DI;

namespace VSS.TRex.Filters.Models
{
  /// <summary>
  /// FilteredMultiplePassInfo records all the information that a filtering operation
  ///   selected from an IC grid cell containing all the recorded machine passes.
  /// </summary>
  public struct FilteredMultiplePassInfo : IEquatable<FilteredMultiplePassInfo>
  {
    /// <summary>
    /// PassCount keeps track of the actual number of passes in the list
    /// </summary>
    public int PassCount;

    /// <summary>
    /// The set of passes selected by the filtering operation
    /// </summary>
    public FilteredPassData[] FilteredPassData;

    private int CellPassAggregationListSizeIncrement() => DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT", Consts.kVlpdpsNodeCellPassAggregationListSizeIncrementDefault);

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


    /*
function TICFilteredMultiplePassInfo.LastPassValidAmp: TICVibrationAmplitude;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if Amplitude<> kICNullAmplitudeValue then
   begin
      Result := Amplitude;
      Exit;
    end;

Result := kICNullAmplitudeValue;
end;


procedure TICFilteredMultiplePassInfo.LastPassValidCCVDetails(var aCCV, aTarget : TICCCVValue);
var
i :Integer;
begin
aCCV := kICNullCCVValue;
aTarget := kICNullCCVValue;
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
begin

  if (FilteredPassData[i].TargetValues.TargetCCV<> kICNullCCVValue) and(aTarget = kICNullCCVValue)  then
  atarget:= FilteredPassData[i].TargetValues.TargetCCV; // just in case ccv is missing but not target

  if CCV<> kICNullCCVValue then
   begin
      aCCV := CCV;
      atarget:= FilteredPassData[i].TargetValues.TargetCCV; // update target with this record
      Exit;
    end;
end;

end;

function TICFilteredMultiplePassInfo.LastPassValidCCA: TICCCAValue;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if CCA<> kICNullCCA then
   begin
      Result := CCA;
      Exit;
    end;

Result := kICNullCCA;

end;

procedure TICFilteredMultiplePassInfo.LastPassValidCCADetails(var aCCA: TICCCAValue; var aTarget : TICCCAMinPassesValue);
var
i :Integer;
begin
aCCA := kICNullCCA;
aTarget :=  kICNullCCATarget;
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
begin
  if (FilteredPassData[i].TargetValues.TargetCCA<> kICNullCCATarget) and(aTarget = kICNullCCATarget)  then
  aTarget:= FilteredPassData[i].TargetValues.TargetCCA; // just in case mdp is missing but not target
  if CCA<> kICNullCCA then
   begin
      aCCA := CCA;
      aTarget:= FilteredPassData[i].TargetValues.TargetCCA;
      Exit;
    end;
end;

end;

function TICFilteredMultiplePassInfo.LastPassValidCCV :TICCCVValue;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if CCV<> kICNullCCVValue then
   begin
      Result := CCV;
      Exit;
    end;

Result := kICNullCCVValue;
end;

function TICFilteredMultiplePassInfo.LastPassValidCCVPercentage :double;
var
i :Integer;
CCVtarget:TICCCVValue;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i] do
  if FilteredPass.CCV<> kICNullCCVValue then
   begin
      CCVtarget:= TargetValues.TargetCCV;
      if (CCVtarget<> 0) and(CCVtarget<> kICNullCCVValue) then
     Result := FilteredPass.CCV / CCVtarget
      else
        Result := kNullCCVPercentage;
      Exit;
    end;

Result := kNullCCVPercentage;
end;

function TICFilteredMultiplePassInfo.LastPassValidFreq: TICVibrationFrequency;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if Frequency<> kICNullFrequencyValue then
   begin
      Result := Frequency;
      Exit;
    end;

Result := kICNullFrequencyValue;
end;

function TICFilteredMultiplePassInfo.LastPassValidGPSMode: TICGPSMode;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if GPSMode<> kICNUllGPSModeValue then
   begin
      Result := GPSMode;
      Exit;
    end;

Result := kICNUllGPSModeValue;
end;

function TICFilteredMultiplePassInfo.LastPassValidMDP: TICMDPValue;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if MDP<> kICNullMDPValue then
   begin
      Result := MDP;
      Exit;
    end;

Result := kICNullMDPValue;
end;


procedure TICFilteredMultiplePassInfo.LastPassValidMDPDetails(var aMDP, aTarget : TICMDPValue);
var
i :Integer;
begin
aMDP := kICNullMDPValue; aTarget :=  kICNullMDPValue;
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
begin
  if (FilteredPassData[i].TargetValues.TargetMDP<> kICNullMDPValue) and(aTarget = kICNullMDPValue)  then
  atarget:= FilteredPassData[i].TargetValues.TargetCCV; // just in case mdp is missing but not target

  if MDP<> kICNullMDPValue then
   begin
      aMDP := MDP;
      aTarget:= FilteredPassData[i].TargetValues.TargetMDP;
      Exit;
    end;
end;
end;


function TICFilteredMultiplePassInfo.LastPassValidMDPPercentage: double;
var
i :Integer;
MDPtarget:TICMDPValue;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i] do
  if FilteredPass.MDP<> kICNullMDPValue then
   begin
      MDPtarget:= TargetValues.TargetMDP;
      if (MDPtarget<> 0) and(MDPtarget<> kICNullMDPValue) then
     Result := FilteredPass.MDP / MDPtarget
      else
        Result := kNullMDPPercentage;
      Exit;
    end;

Result := kNullMDPPercentage;
end;

function TICFilteredMultiplePassInfo.LastPassValidRadioLatency: TICRadioLatency;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if RadioLatency<> kICNullRadioLatency then
   begin
      Result := RadioLatency;
      Exit;
    end;

Result := kICNullRadioLatency;
end;

function TICFilteredMultiplePassInfo.LastPassValidRMV: TICRMVValue;
var
i :Integer;
begin
for i := FPassCount - 1 downto 0 do
with FilteredPassData[i].FilteredPass do
  if RMV<> kICNullRMVValue then
   begin
      Result := RMV;
      Exit;
    end;

Result := kICNullRMVValue;
end;
*/

    public DateTime LowestPassTime()
    {
      float TempHeight = Consts.NullHeight;
      DateTime Result = DateTime.MinValue;

      for (int i = PassCount - 1; i >= 0; i--)
        //  with FilteredPassData[i].FilteredPass do
        if (TempHeight == Consts.NullHeight)
        {
          TempHeight = FilteredPassData[i].FilteredPass.Height;
          Result = FilteredPassData[i].FilteredPass.Time;
        }
        else if (FilteredPassData[i].FilteredPass.Height < TempHeight)
          Result = FilteredPassData[i].FilteredPass.Time;

      return Result;
    }

    /*
function TICFilteredMultiplePassInfo.LastPassValidTemperature :TICMaterialTemperature;
var
I :Integer;
begin
for i := FPassCount - 1 downto 0 do
  with FilteredPassData[i].FilteredPass do
    if MaterialTemperature<> kICNullMaterialTempValue then
     begin
        Result := MaterialTemperature;
        Exit;
      end;

Result := kICNullMaterialTempValue;
end;

procedure TICFilteredMultiplePassInfo.SetPassesLength(Value: Integer);
begin
SetLength(FilteredPassData, Value);
end;
*/

    /// <summary>
    /// Serialises content of the cell to the writer
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
    /// Serialises content of the cell from the writer
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
      else
      {
        FilteredPassData = new FilteredPassData[0];
      }
    }

    public bool Equals(FilteredMultiplePassInfo other)
    {
      return PassCount == other.PassCount && 
             (Equals(FilteredPassData, other.FilteredPassData) ||
              (FilteredPassData != null && other.FilteredPassData != null && FilteredPassData.SequenceEqual(other.FilteredPassData)));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is FilteredMultiplePassInfo && Equals((FilteredMultiplePassInfo) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (PassCount * 397) ^ (FilteredPassData != null ? FilteredPassData.GetHashCode() : 0);
      }
    }
  }
}
