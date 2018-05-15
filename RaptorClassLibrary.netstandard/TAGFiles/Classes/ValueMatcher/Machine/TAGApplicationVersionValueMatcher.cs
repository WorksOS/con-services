using System.Text;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the machine control application version TAG
    /// </summary>
    public class TAGApplicationVersionValueMatcher : TAGValueMatcher
    {
        public TAGApplicationVersionValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileApplicationVersion };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.ApplicationVersion = Encoding.ASCII.GetString(value);

            return true;
        }
    }
}
