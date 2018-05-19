using System;
using VSS.TRex.DI;
using VSS.TRex.Servers.Compute;

namespace VSS.TRex.Server.MutableData
{
  class Program
  {
  private static void DependencyInjection()
    {
      DIImplementation.New().AddLogging().Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      var server = new TagProcComputeServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
