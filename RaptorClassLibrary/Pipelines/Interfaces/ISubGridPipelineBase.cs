using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Pipelines.Interfaces
{
    public interface ISubGridPipelineBase
    {
        void Abort();

        bool PipelineAborted { get; set; }

        long DataModelID { get; set; }

        void SubgridProcessed();
    }
}
