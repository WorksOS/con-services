using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    /// <summary>
    /// Handle the flag indicating the compactor machine is using the Caterpillar Compaction Algorithm
    /// </summary>
    public class TAGUsingCCAValueMatcher : TAGValueMatcher
    {
        public TAGUsingCCAValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagUsingCCA };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.UsingCCA = value != 0;

            return true;
        }
    }
}
