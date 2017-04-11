using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGWheelWidthValueMatcher : TAGValueMatcher
    {
        public TAGWheelWidthValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileWheelWidthTag };
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            // Value is absolute wheel width expressed in

            if (valueType.Type == TAGDataType.tIEEEDouble)
            {
                valueSink.MachineWheelWidth = value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
