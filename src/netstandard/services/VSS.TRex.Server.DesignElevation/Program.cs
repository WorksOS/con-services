using System;
using VSS.TRex.Common.Utilities;
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

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      var server = new CalculateDesignElevationsServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
