using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for Wheel axle positioning
    /// </summary>
    public class TAGLeftRightWheelValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightWheelValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftWheelTag, TAGValueNames.kTagFileRightWheelTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            bool result = true;

            if (valueType.Name == TAGValueNames.kTagFileLeftWheelTag)
            {
                state.WheelSide = TAGValueSide.Left;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightWheelTag)
            {
                state.WheelSide = TAGValueSide.Right;
            }
            else
            {
                result = false;
            }

            return result;
        }

    }
}
