namespace VSS.TRex.TAGFiles.Classes.States
{
    /// <summary>
    /// Handles prescanning TAG values an extracting the first encountered accurate grid point positions
    /// </summary>
    public class TAGProcessorPreScanState : TAGProcessorStateBase
    {
        public TAGProcessorPreScanState()
        {
        }

        public override bool ProcessEpochContext()
        {
            if (!HaveFirstAccurateGridEpochEndPoints)
                base.ProcessEpochContext();

            ProcessedEpochCount++;

            return true; // Force reading of entire TAG file contents
        }
    }
}
