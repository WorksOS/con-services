using System;
using VSS.Velociraptor.DesignProfiling.Servers.Client;

namespace VSS.TRex.Server.DesignElevation
{
  class Program
  {
    static void Main(string[] args)
    {
      var server = new CalculateDesignElevationsServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
