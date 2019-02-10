using VSS.TRex.Common.Types;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the flag indicating when the machine is recording minimum elevation mapping information
    /// </summary>
    public class TAGElevationMappingModeValueMatcher : TAGValueMatcher
    {
        public TAGElevationMappingModeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileElevationMappingModeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t8bitUInt && 
                (value == (byte)ElevationMappingMode.LatestElevation || 
                 value == (byte)ElevationMappingMode.MinimumElevation))
            {
                valueSink.SetElevationMappingModeState((ElevationMappingMode)value);
                return true;
            }

            return false;
        }
    }
}
