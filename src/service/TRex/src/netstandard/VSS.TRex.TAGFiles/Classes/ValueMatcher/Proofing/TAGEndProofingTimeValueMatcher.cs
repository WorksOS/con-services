using VSS.TRex.Common.Time;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Proofing
{
  /// <summary>
  /// Handles proofing run start week and time TAGs
  /// </summary>
  public class TAGEndProofingTimeValueMatcher : TAGValueMatcher
  {
    public TAGEndProofingTimeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
    {
    }

    private static readonly string[] valueTypes = { TAGValueNames.kTagFileStartProofingTimeTag, TAGValueNames.kTagFileStartProofingWeekTag };

    public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      if (valueType.Name == TAGValueNames.kTagFileStartProofingTimeTag)
      {
        // Every time record marks the end of the collected data for an epoch
        // Thus, we instruct the value sink to process its context whenever we receive a time value.
        if (state.HaveSeenAProofingRunTimeValue && !valueSink.ProcessEpochContext())
          return false;

        if (valueType.Type == TAGDataType.t32bitUInt)
        {
          valueSink.StartProofingTime = value; // Time value is GPS milliseconds since start of week
          state.HaveSeenAProofingRunTimeValue = true;
          result = true;
        }
      }

      if (valueType.Name == TAGValueNames.kTagFileStartProofingWeekTag)
      {
        if (valueType.Type == TAGDataType.t16bitUInt)
        {
          valueSink.StartProofingWeek = (short) value;
          state.HaveSeenAProofingRunWeekValue = true;
          result = true;
        }
      }

      // if we have seen both a GPS week and time then we can compute the DataTime
      // value for the value sink
      if (result && state.HaveSeenAProofingRunTimeValue && state.HaveSeenAProofingRunWeekValue)
      {
        valueSink.StartProofingDataTime = GPS.GPSOriginTimeToDateTime(valueSink.StartProofingWeek, valueSink.StartProofingTime);
      }

      return result;
    }
  }
}
