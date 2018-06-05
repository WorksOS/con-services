using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Executors.Tasks.Interfaces
{
    public interface ITask  
    {
        void Cancel();

        bool TransferResponse(object response);

        bool TransferResponses(object[] responses);

        GridDataType GridDataType { get; set; }

        string TRexNodeID { get; set; }

        ISubGridPipelineBase PipeLine { get; set; }

        bool IsCancelled { get; set; }
    }
}
