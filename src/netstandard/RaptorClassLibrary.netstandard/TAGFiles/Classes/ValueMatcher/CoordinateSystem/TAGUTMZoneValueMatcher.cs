using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.CoordinateSystem
{
    /// <summary>
    /// Handles the Universal Tranverse Mercator project zone being used for grid coordinates written into the TAG file data
    /// </summary>
    public class TAGUTMZoneValueMatcher : TAGValueMatcher
    {
        public TAGUTMZoneValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagUTMZone };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t8bitUInt)
            {
                valueSink.UTMZone = (byte)value;

                if (!state.HaveSeenAUTMZone)
                {
                    valueSink.UTMZoneAtFirstPosition = (byte)value;

                    state.HaveSeenAUTMZone = true;
                }

                return true;
            }

            return false;            
        }
    }
}
