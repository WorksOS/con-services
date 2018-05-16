namespace VSS.TRex.TAGFiles.Classes.Sinks
{
    /// <summary>
    /// Specialised TAG value sink that determines if the prerequisites for processing this information into 
    /// visionlink have been met
    /// </summary>
    public class TAGVisionLinkPrerequisitesValueSink : TAGValueSink
    {
        /// <summary>
        /// Constructor -> proxied to base
        /// </summary>
        /// <param name="processor"></param>
        public TAGVisionLinkPrerequisitesValueSink(TAGProcessorStateBase processor) : base(processor)
        {
        }

        public override bool Aborting()
        {
            // Allow reading of entire contents of TAG file, until the
            // aborting test succeeds or the end of the file is reached.
            return false;
        }

        public override bool Finishing()
        {
            // Allow reading of entire contents of TAG file, until the
            // aborting test succeeds or the end of the file is reached.

            //Check if we need to process a final context to be added into the coordinate array
            return !ValueMatcherState.HaveSeenATimeValue || Processor.ProcessEpochContext();
        }
    }
}
