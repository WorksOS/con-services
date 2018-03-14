using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileLeftWheelTag, TAGValueNames.kTagFileRightWheelTag };
        }

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
