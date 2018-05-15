using System.Linq;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// TAGValueMatcher is a base class used to produce a set of derived classes
    /// that know how to process the value information as it is read from the tag file.
    /// </summary>
    public abstract class TAGValueMatcher
    {
        /// <summary>
        /// The value sink that all TAG values read in will be sent to
        /// </summary>
        protected TAGProcessorStateBase valueSink;

        /// <summary>
        /// The state machine state updated as new TAG values are read in. Value matchers update this state
        /// which in turn responds by emitted events to trigger further processing.
        /// </summary>
        protected TAGValueMatcherState state { get; set; }

        /// <summary>
        /// MatchesValueType determines if this value matcher is interested in fields with a field name of AValueName
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public bool MatchesValueType(TAGDictionaryItem valueType) => MatchedValueTypes().Contains(valueType.Name);

        /// <summary>
        /// The set of TAG values a TAG value matcher advertises it is interested in.
        /// </summary>
        /// <returns></returns>
        public abstract string[] MatchedValueTypes();

        // Process*Value is given the value to deal with. If the value is OK then it
        // passes it to the value sink. If there was an issue with the value then the
        // value matcher returns false.

        /// <summary>
        /// Processes an integer TAG value
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessIntegerValue(TAGDictionaryItem valueType, int value) => false;

        /// <summary>
        /// Processes an unsign integer TAG value
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value) => false;

        /// <summary>
        /// Processes a double TAG value
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessDoubleValue(TAGDictionaryItem valueType, double value) => false;

        /// <summary>
        /// Processes an empty TAG value (a value with a name but no value at all - the TAG itself is the value)
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public virtual bool ProcessEmptyValue(TAGDictionaryItem valueType) => false;

        /// <summary>
        /// Processes an ANSI string TAG value
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value) => false;

        /// <summary>
        /// Processes an unicode TAG value
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessUnicodeStringValue(TAGDictionaryItem valueType, string value) => false;

        /// <summary>
        /// TAGValueMatcher constructor. The stae machine state and the sink to send matched values to are
        /// provided as arguments.
        /// </summary>
        /// <param name="valueSink"></param>
        /// <param name="state"></param>
        public TAGValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state)
        {
            this.valueSink = valueSink;
            this.state = state;
        }
    }
}

