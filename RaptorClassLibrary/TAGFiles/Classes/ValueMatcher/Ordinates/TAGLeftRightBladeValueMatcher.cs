using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileLeftTag, TAGValueNames.kTagFileRightTag };
        }

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
