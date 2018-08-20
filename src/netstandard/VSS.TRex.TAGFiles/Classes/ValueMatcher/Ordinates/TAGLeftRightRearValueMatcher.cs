using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for rear axle positioning
    /// </summary>
    public class TAGLeftRightRearValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightRearValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftRearTag, TAGValueNames.kTagFileRightRearTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            bool result = true;

            if (valueType.Name == TAGValueNames.kTagFileLeftRearTag)
            {
                state.RearSide = TAGValueSide.Left;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightRearTag)
            {
                state.RearSide = TAGValueSide.Right;
            }
            else
            {
                result = false;
            }

            return result;
        }

    }
}
