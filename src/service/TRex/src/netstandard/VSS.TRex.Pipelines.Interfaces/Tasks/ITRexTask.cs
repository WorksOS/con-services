using System;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Interfaces.Tasks
{
    public interface ITRexTask  
    {
        void Cancel();

        bool TransferResponse(object response);

        bool TransferResponses(object[] responses);

        GridDataType GridDataType { get; set; }

        string TRexNodeID { get; set; }

        ISubGridPipelineBase PipeLine { get; set; }

        bool IsCancelled { get; set; }
        Guid RequestDescriptor { get; set; }
    }
}
