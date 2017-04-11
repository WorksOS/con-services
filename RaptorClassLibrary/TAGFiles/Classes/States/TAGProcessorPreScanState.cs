using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.States
{
    /// <summary>
    /// Handles prescanning TAG values an extracting the first encountered accurate grid point positions
    /// </summary>
    class TAGProcessorPreScanState : TAGProcessorStateBase
    {
        public TAGProcessorPreScanState() : base()
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
