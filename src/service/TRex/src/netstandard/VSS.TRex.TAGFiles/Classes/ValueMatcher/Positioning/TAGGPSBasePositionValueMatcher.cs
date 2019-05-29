using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    public class TAGGPSBasePositionValueMatcher : TAGValueMatcher
    {
        public TAGGPSBasePositionValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagGPSBasePosition };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            state.GPSBasePositionReportingHaveStarted = true;

            return true;
        }
    }
}
