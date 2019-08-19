using System.Threading.Tasks;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
  public interface IOverrideEventExecutor
  {
    Task<OverrideEventResponse> ExecuteAsync(OverrideEventRequestArgument arg);
  }
}
