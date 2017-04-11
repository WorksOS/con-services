using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCAValueMatcher : TAGValueMatcher
    {
        public TAGCCAValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileICCCATag };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            valueSink.SetICCCAValue(CellPass.NullCCA);

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t8bitUInt)
            {
                return false;
            }

            valueSink.SetICCCAValue((short)value);

            return true;
        }
    }
}
