using System;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace VSS.TRex.Server.PSNode
{
  class Program
  {
    static void Main(string[] args)
    {
      var server = new RaptorSubGridProcessingServer();
      Console.WriteLine($"Spatial Division {RaptorServerConfig.Instance().SpatialSubdivisionDescriptor}");
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
