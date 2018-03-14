using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGDesignValueMatcher : TAGValueMatcher
    {
        public TAGDesignValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileDesignTag };
        }

        public override bool ProcessUnicodeStringValue(TAGDictionaryItem valueType, string value)
        {
            valueSink.Design = value;

            return true;
        }
    }
}
