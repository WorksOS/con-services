using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the machine speed value reported from the machine ECM
    /// </summary>
    public class TAGMachineSpeedValueMatcher : TAGValueMatcher
    {
        public TAGMachineSpeedValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagMachineSpeed };
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            state.HaveSeenMachineSpeed = true;
            valueSink.SetICMachineSpeedValue(value);

            return true;
        }
    }
}
