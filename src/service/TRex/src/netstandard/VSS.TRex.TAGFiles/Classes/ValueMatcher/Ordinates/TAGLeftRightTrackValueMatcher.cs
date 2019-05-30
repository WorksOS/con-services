using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for Track axle positioning
    /// </summary>
    public class TAGLeftRightTrackValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightTrackValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftTrackTag, TAGValueNames.kTagFileRightTrackTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            bool result = false;

            if (valueType.Name == TAGValueNames.kTagFileLeftTrackTag)
            {
                state.TrackSide = TAGValueSide.Left;
                result = true;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightTrackTag)
            {
                state.TrackSide = TAGValueSide.Right;
                result = true;
            }

            return result;
        }

    }
}
