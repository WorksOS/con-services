using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for Track axle positioning
    /// </summary>
    public class TAGLeftRightTrackValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightTrackValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftTrackTag, TAGValueNames.kTagFileRightTrackTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            bool result = true;

            if (valueType.Name == TAGValueNames.kTagFileLeftTrackTag)
            {
                state.TrackSide = TAGValueSide.Left;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightTrackTag)
            {
                state.TrackSide = TAGValueSide.Right;
            }
            else
            {
                result = false;
            }

            return result;
        }

    }
}
