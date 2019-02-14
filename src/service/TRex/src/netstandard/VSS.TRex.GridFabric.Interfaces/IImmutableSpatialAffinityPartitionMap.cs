using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface IImmutableSpatialAffinityPartitionMap
  {
    bool[] PrimaryPartitions();
  }
}
