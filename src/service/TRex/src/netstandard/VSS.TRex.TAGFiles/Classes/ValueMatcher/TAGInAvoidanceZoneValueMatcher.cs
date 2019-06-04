using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// Handle the avoidance zone entry/exit state reported by the machine
    /// </summary>
    public class TAGInAvoidanceZoneValueMatcher : TAGValueMatcher
    {
        public TAGInAvoidanceZoneValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagInAvoidZone };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt && value <= 3) // Check only the two least significant bits are set
            {
              valueSink.SetInAvoidZoneState((byte) value);
              result = true;
            }

            return result;
        }
    }
}
