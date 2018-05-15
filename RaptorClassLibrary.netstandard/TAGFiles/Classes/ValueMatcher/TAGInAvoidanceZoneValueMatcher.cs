using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// Handle the avoidance zone entry/exit state reported by the machine
    /// </summary>
    public class TAGInAvoidanceZoneValueMatcher : TAGValueMatcher
    {
        public TAGInAvoidanceZoneValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagInAvoidZone };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (!(valueType.Type == TAGDataType.t4bitUInt && value <= 3)) // Check only the two least significant bits are set
            {
                return false;
            }

            valueSink.SetInAvoidZoneState((byte)value);

            return true;
        }
    }
}
