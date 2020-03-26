using System;
using VSS.TRex.GridFabric;

namespace VSS.TRex.SubGrids.Interfaces
{
  public interface ISubGridProgressiveResponseRequestComputeFuncArgument
  {
    Guid NodeId { get; set; }

    /// <summary>
    /// A common descriptor that may be supplied by the argument consumer to hold an
    /// externally provided Guid identifier for the request
    /// </summary>
    Guid RequestDescriptor { get; set; }

    ISerialisedByteArrayWrapper Payload { get; set; }
  }
}
