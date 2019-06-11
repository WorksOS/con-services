using System.Text;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Proofing
{
    /// <summary>
    /// Handles proofing run starting TAGs
    /// </summary>
    public class TAGStartProofingValueMatcher : TAGValueMatcher
    {
        public TAGStartProofingValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileStartProofingTag };

        public override string[] MatchedValueTypes() => valueTypes;

        private void SetProofingRunStartTime(TAGValueMatcherState state, TAGProcessorStateBase valueSink)
        {
            if (state.HaveSeenATimeValue && state.HaveSeenAWeekValue)
                valueSink.StartProofingDataTime = valueSink.DataTime;
        }

        public override bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, string value)
        {
            if (!state.ProofingRunProcessed)
                state.ProofingRunProcessed = valueSink.ProcessEpochContext();

            valueSink.StartProofing = value;

            SetProofingRunStartTime(state, valueSink);

            return true;
        }

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            if (!state.ProofingRunProcessed)
                state.ProofingRunProcessed = valueSink.ProcessEpochContext();

            valueSink.StartProofing = "";

            SetProofingRunStartTime(state, valueSink);

            return true;
        }
    }
}
