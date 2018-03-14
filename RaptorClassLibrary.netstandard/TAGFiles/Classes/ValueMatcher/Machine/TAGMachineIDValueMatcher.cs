using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Matches machine ID values
    /// </summary>
    public class TAGMachineIDValueMatcher : TAGValueMatcher
    {
        public TAGMachineIDValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileMachineIDTag };
        }

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.MachineID = Encoding.ASCII.GetString(value);
            state.HaveSeenAMachineID = true;

            return true;
        }
    }
}
