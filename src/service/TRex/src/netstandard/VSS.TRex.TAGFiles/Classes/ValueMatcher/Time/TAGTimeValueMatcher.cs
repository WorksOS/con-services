using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Common.Time;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Time
{
    /// <summary>
    /// Handles TIME and WEEK values (both absolute and delta)
    /// </summary>
    public class TAGTimeValueMatcher : TAGValueMatcher
    {
        public TAGTimeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileTimeTag, TAGValueNames.kTagFileWeekTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Name == TAGValueNames.kTagFileTimeTag)
            {
                // Every time record marks the end of the collected data for an epoch
                // Thus, we instruct the value sink to process its context whenever we receive
                // a time value.
                if (state.HaveSeenATimeValue)
                    valueSink.ProcessEpochContext();

                switch (valueType.Type)
                {
                    case TAGDataType.t32bitUInt:
                        // {$IFDEF DENSE_TAG_FILE_LOGGING}
                        // SIGLogProcessMessage.Publish(Self, Format('Time Origin Update: Incremented Time: %d, new Origin: %d, Delta: %d', { SKIP}
                        //                                           [FValueSink.GPSWeekTime, Value, Value - FValueSink.GPSWeekTime]), slpmcMessage);
                        // {$ENDIF}

                        valueSink.GPSWeekTime = value;                    // Time value is GPS milliseconds since start of week
                        state.HaveSeenATimeValue = true;
                        result = true;
                        break;

                    case TAGDataType.t4bitUInt:
                        valueSink.GPSWeekTime = valueSink.GPSWeekTime + 100 * value; // Time value is tenths of seconds delta from previous time
                        state.HaveSeenATimeValue = true;
                        result = true;
                        break;
                }
            }
            else if (valueType.Name == TAGValueNames.kTagFileWeekTag)
            {
                if (valueType.Type == TAGDataType.t16bitUInt)
                {
                    valueSink.GPSWeekNumber = (short) value;
                    state.HaveSeenAWeekValue = true;
                    result = true;
                }
            }

            // if we have seen both a GPS week and time then we can compute the DataTime
            // value for the value sink
            if (state.HaveSeenATimeValue && state.HaveSeenAWeekValue)
                valueSink.DataTime = GPS.GPSOriginTimeToDateTime(valueSink.GPSWeekNumber, valueSink.GPSWeekTime);

            return result;
        }
    }
}
