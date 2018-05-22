using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Servers.Compute;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Classes;

namespace VSS.TRex.Server.MutableData
{
  class Program
  {
  private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ITFAProxy>(new TFAProxy()))
        .Complete();
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
