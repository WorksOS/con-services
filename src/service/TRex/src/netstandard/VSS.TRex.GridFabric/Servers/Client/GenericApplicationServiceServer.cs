using System;

namespace VSS.TRex.GridFabric.Servers.Client
{
  /// <summary>
  /// An application service server that can take a generic action to perform in the context of an application service service
  /// </summary>
  public class GenericApplicationServiceServer : ApplicationServiceServer
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public GenericApplicationServiceServer()
    {
    }

    /// <summary>
    /// Generic implementation of a function lambda executed in the context of the TRexApplicationServicesServer
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static TResult PerformAction<TResult>(Func<TResult> func) => func();
  }
}
