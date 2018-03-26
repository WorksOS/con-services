using System;
using System.Net.Mime;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace VSS.TRex.Server.MutableData
{
  class Program
  {
    static void Main(string[] args)
    {
      var server = new RaptorTAGProcComputeServer();
      Console.WriteLine($"Spatial Division {RaptorServerConfig.Instance().SpatialSubdivisionDescriptor}");
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
