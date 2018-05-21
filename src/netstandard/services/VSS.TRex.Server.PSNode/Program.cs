using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Servers.Compute;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Server.PSNode
{
  class Program
  {
    private static void DependencyInjection()
    {
      DIImplementation.New().AddLogging().Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory())).Complete();
    }

    static void Main(string[] args)
    { 
      DependencyInjection();

      var server = new SubGridProcessingServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
