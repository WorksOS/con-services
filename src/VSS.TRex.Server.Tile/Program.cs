using System;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.TRex.Server.Application
{
  class Program
  {
    static void Main(string[] args)
    {
      var server = new RaptorApplicationServiceServer();
      Console.WriteLine($"Spatial Division {RaptorServerConfig.Instance().SpatialSubdivisionDescriptor}");
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
