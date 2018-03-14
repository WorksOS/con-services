using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    ///  Handles the blade of ground flag as reported fom the amchine
    /// </summary>
    public class TAGBladeOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGBladeOnGroundValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileBladeOnGroundTag };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }


            if (!Enum.IsDefined(typeof(OnGroundState), (int)value))
            {
                return false;
            }

            valueSink.SetOnGround((OnGroundState)value);
            return true;
        }
    }
}
