using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks.Interfaces
{
    public interface ITask
    {
        void Cancel();
        bool TransferResponse(object response);

        GridDataType GridDataType { get; set; }
    }
}