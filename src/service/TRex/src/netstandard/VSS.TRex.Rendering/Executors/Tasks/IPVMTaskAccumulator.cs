using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public interface IPVMTaskAccumulator
  {
    bool Transcribe(IClientLeafSubGrid[] subGridResponses);
  }
}
