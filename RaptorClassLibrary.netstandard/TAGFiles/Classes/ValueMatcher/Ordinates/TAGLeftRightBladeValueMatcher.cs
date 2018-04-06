using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for blade tip positioning
    /// </summary>
    public class TAGLeftRightBladeValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightBladeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftTag, TAGValueNames.kTagFileRightTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            bool result = true;

            if (valueType.Name == TAGValueNames.kTagFileLeftTag)
            {
                state.Side = TAGValueSide.Left;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightTag)
            {
                state.Side = TAGValueSide.Right;
            }
            else
            {
                result = false;
            }

            return result;
        }

    }
}
