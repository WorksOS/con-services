using System;
using VSS.TRex.Types;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface ISiteModelMachineAffinityKey : IProjectAffinity
  {
    Guid AssetUID { get; set; }

    FileSystemStreamType StreamType { get; set; }
  }
}
