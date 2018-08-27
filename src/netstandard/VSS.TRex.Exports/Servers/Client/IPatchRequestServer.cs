using VSS.TRex.Exports.Patches;
using VSS.TRex.Exports.Patches.GridFabric;

namespace VSS.TRex.Exports.Servers.Client
{
  public interface IPatchRequestServer
  {
    /// <summary>
    /// Generate a patch of subgrids given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    PatchResult Execute(PatchRequestArgument argument);
  }
}
