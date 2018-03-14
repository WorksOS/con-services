using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for rear axle positioning
    /// </summary>
    public class TAGLeftRightRearValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightRearValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileLeftRearTag, TAGValueNames.kTagFileRightRearTag };
        }

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
