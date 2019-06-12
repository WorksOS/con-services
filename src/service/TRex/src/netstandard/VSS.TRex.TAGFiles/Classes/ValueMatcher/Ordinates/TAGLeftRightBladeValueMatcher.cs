using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for blade tip positioning
    /// </summary>
    public class TAGLeftRightBladeValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightBladeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftTag, TAGValueNames.kTagFileRightTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            bool result = false;

            if (valueType.Name == TAGValueNames.kTagFileLeftTag)
            {
                state.Side = TAGValueSide.Left;
                result = true;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightTag)
            {
                state.Side = TAGValueSide.Right;
                result = true;
            }

            return result;
        }

    }
}
