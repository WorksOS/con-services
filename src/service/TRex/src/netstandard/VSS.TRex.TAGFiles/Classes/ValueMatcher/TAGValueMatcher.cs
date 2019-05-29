using VSS.TRex.TAGFiles.Classes.States;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// TAGValueMatcher is a base class used to produce a set of derived classes
    /// that know how to process the value information as it is read from the tag file.
    /// </summary>
    public abstract class TAGValueMatcher
    {
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
        /// <param name="state"></param>
        /// <param name="valueSink"></param>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, int value) => false;

        /// <summary>
        /// Processes an unsigned integer TAG value
        /// </summary>
        /// <param name="state"></param>
        /// <param name="valueSink"></param>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value) => false;

        /// <summary>
        /// Processes a double TAG value
        /// </summary>
        /// <param name="state"></param>
        /// <param name="valueSink"></param>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessDoubleValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, double value) => false;

        /// <summary>
        /// Processes an empty TAG value (a value with a name but no value at all - the TAG itself is the value)
        /// </summary>
        /// <param name="state"></param>
        /// <param name="valueSink"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public virtual bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType) => false;

        /// <summary>
        /// Processes an ANSI string TAG value
        /// </summary>
        /// <param name="state"></param>
        /// <param name="valueSink"></param>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, byte[] value) => false;

        /// <summary>
        /// Processes an unicode TAG value
        /// </summary>
        /// <param name="state"></param>
        /// <param name="valueSink"></param>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ProcessUnicodeStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, string value) => false;

        /// <summary>
        /// TAGValueMatcher constructor. The state machine state and the sink to send matched values to are
        /// provided as arguments.
        /// </summary>
        public TAGValueMatcher()
        {
        }
    }
}

