using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.Utilities;

namespace VSS.TRex.TAGFiles.Classes.Sinks
{
    /// <summary>
    /// Implements a sink against all defined TAG values.
    /// </summary>
    public class TAGValueSink : TAGValueSinkBase
    {
        /// <summary>
        /// Processor responsible for accepting TAG values matched by the TAG value matcher
        /// </summary>
        protected TAGProcessorStateBase Processor { get; set; }

        /// <summary>
        /// The set of value matchers available to match TAG values being accepted
        /// </summary>
        private Dictionary<string, TAGValueMatcher> ValueMatchers { get; set; } = new Dictionary<string, TAGValueMatcher>();

        /// <summary>
        /// Returns the list of TAGs that are supported by this instance of the TAG value sink
        /// </summary>
        public string[] InstantiatedTAGs => ValueMatchers.Keys.ToArray();

        /// <summary>
        /// Local value matcher state that the TAG value matchers use to coordinate values before sending them to the procesor
        /// </summary>
        protected TAGValueMatcherState ValueMatcherState { set; get; } = new TAGValueMatcherState();

        /// <summary>
        /// Locate all value matcher classes and add them to the value matchers list using reflection (or just manually as below)
        /// </summary>
        private void InitialiseValueMatchers()
        {
            // Get all the value matcher classes that exist in the assembly. These are all classes that
            // descend from TAGValueMatcher
            List<Type> matchers = TypesHelper.FindAllDerivedTypes<TAGValueMatcher>();

            // Iterate through those types and create each on in turn, query the TAG types from it that the matcher supports and
            // then register the value matcher instance against those TAGs to allwo the TAG file processor to locate matcher for 
            // TAGS
            foreach (Type t in matchers)
            {
                TAGValueMatcher matcher = (TAGValueMatcher)Activator.CreateInstance(t, Processor, ValueMatcherState);

                foreach (string tag in matcher.MatchedValueTypes())
                {
                    ValueMatchers.Add(tag, matcher);
                }
            }

            // For each value matcher, register the value the matcher class supports into the value matcher dictionary
            //matchers.Select(matcher => { ((string[])(matcher.GetMethod("matchedValueTypes").Invoke(null, null))).ForEach(value => ValueMatchers.Add(value, matcher)); });
        }

        public TAGValueSink(TAGProcessorStateBase processor)
        {
            Processor = processor;

            // Populate the Value matchers list with those necessary to read the file
            InitialiseValueMatchers();
        }

        public override bool Starting() => true;

        public override bool Finishing()
        {
            //Check if we need to process a final context
            return !ValueMatcherState.HaveSeenATimeValue || Processor.ProcessEpochContext();
        }

        public override void ReadANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessANSIStringValue(valueType, value);
            }
        }

        public override void ReadEmptyValue(TAGDictionaryItem valueType)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessEmptyValue(valueType);
            }
        }

        public override void ReadIEEEDoubleValue(TAGDictionaryItem valueType, double value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessDoubleValue(valueType, value);
            }
        }

        public override void ReadIEEESingleValue(TAGDictionaryItem valueType, float value)
        {
           // Don't care - apparently no Single TAG names have ever been defined
        }

        public override void ReadIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessIntegerValue(valueType, value);
            }
        }

        public override void ReadUnicodeStringValue(TAGDictionaryItem valueType, string value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessUnicodeStringValue(valueType, value);
            }
        }

        public override void ReadUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessUnsignedIntegerValue(valueType, value);
            }
        }
    }
}
