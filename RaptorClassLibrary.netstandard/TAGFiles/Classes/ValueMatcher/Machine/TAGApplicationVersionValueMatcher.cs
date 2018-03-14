using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the machine control application version TAG
    /// </summary>
    public class TAGApplicationVersionValueMatcher : TAGValueMatcher
    {
        public TAGApplicationVersionValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileApplicationVersion };
        }

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.ApplicationVersion = Encoding.ASCII.GetString(value);

            return true;
        }
    }
}
