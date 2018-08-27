namespace VSS.TRex.TAGFiles.Classes.Sinks
{
    /// <summary>
    /// TAGValueSink defines a sink where values are sent to as they are parsed from the TAG information
    /// </summary>
    public abstract class TAGValueSinkBase
    {
        /// <summary>
        /// Internal flag to controll termination of processing
        /// </summary>
        private bool processingTerminated;

        /// <summary>
        /// Accepts an integer value from the TAG file data
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        public abstract void ReadIntegerValue(TAGDictionaryItem valueType, int value);

        /// <summary>
        /// Accepts an unsigned integer value from the TAG file data
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        public abstract void ReadUnsignedIntegerValue(TAGDictionaryItem valueType, uint value);

        /// <summary>
        /// Accepts an ANSI string value from the TAG file data. The string is presented as an array of
        /// bytes representing the actual data read in from the TAG value.
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        public abstract void ReadANSIStringValue(TAGDictionaryItem valueType, byte[] value);

        /// <summary>
        /// Accepts an Unicode string value from the TAG file data.
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        public abstract void ReadUnicodeStringValue(TAGDictionaryItem valueType, string value);

        /// <summary>
        /// Accepts a IEEE single/float value from the TAG file data
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        public abstract void ReadIEEESingleValue(TAGDictionaryItem valueType, float value);

        /// <summary>
        /// Accepts a IEEE double value from the TAG file data
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        public abstract void ReadIEEEDoubleValue(TAGDictionaryItem valueType, double value);

        /// <summary>
        /// Accepts an empty value from the TAG file data
        /// </summary>
        /// <param name="valueType"></param>
        public abstract void ReadEmptyValue(TAGDictionaryItem valueType);

        /// <summary>
        /// Parsing of TAG file value has begun and will start to be sent to the sink
        /// </summary>
        /// <returns></returns>
        public abstract bool Starting();

        /// <summary>
        /// Parsing of TAG file data has completed
        /// </summary>
        /// <returns></returns>
        public abstract bool Finishing();

        /// <summary>
        /// Aborting is used to indicate the the scanning should stop as we have located
        /// the information we are looking for.
        /// </summary>
        /// <returns></returns>
        public virtual bool Aborting() => false;

        /// <summary>
        /// ProcessingTerminated is used to preemptively stop processing due to some error condition etc.
        /// </summary>
        public virtual bool ProcessingTerminated => processingTerminated;

        /// <summary>
        /// Preemptively teminate processing of TAG values being sent to the sink
        /// </summary>
        public void TerminateProcessing() => processingTerminated = true;
    }
}
