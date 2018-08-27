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
        public TAGStartProofingValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileStartProofingTag };

        public override string[] MatchedValueTypes() => valueTypes;

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
