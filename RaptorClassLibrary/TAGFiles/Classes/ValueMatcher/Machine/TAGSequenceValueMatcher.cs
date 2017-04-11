using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles compactor and hardware pair ID values
    /// </summary>
    public class TAGSequenceValueMatcher : TAGValueMatcher
    {
        public TAGSequenceValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileSequenceTag };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            bool result = valueType.Type == TAGDataType.t32bitUInt;

            if (result)
            {
                valueSink.Sequence = value;
            }

            return result;
        }
    }
}
