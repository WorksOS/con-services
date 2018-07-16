using System;
using VSS.TRex.Designs.Servers.Client;
using VSS.TRex.DI;

namespace VSS.TRex.Server.DesignElevation
{
  class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder.New().AddLogging().Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      var server = new CalculateDesignElevationsServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
