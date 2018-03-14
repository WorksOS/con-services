using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Proofing
{
    /// <summary>
    /// Handles proofing run starting TAGs
    /// </summary>
    public class TAGStartProofingValueMatcher : TAGValueMatcher
    {
        public TAGStartProofingValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileStartProofingTag };
        }

        private void SetProofingRunStartTime()
        {
            if (state.HaveSeenATimeValue && state.HaveSeenAWeekValue)
            {
                valueSink.StartProofingDataTime = valueSink.DataTime;
            }
        }

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            if (!state.ProofingRunProcessed)
            {
                state.ProofingRunProcessed = valueSink.ProcessEpochContext();
            }

            valueSink.StartProofing = Encoding.ASCII.GetString(value);

            SetProofingRunStartTime();

            return true;
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            if (!state.ProofingRunProcessed)
            {
                state.ProofingRunProcessed = valueSink.ProcessEpochContext();
            }

            valueSink.StartProofing = "";

            SetProofingRunStartTime();

            return true;
        }
    }
}
