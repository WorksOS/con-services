using System.Threading.Tasks;
using Apache.Ignite.Core.Binary;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface IBaseRequest : IBinarizable
  {
  }

  /// <summary>
  /// Provides a generic interface that can represent an Ignite request in terms of its argument and results
  /// </summary>
  public interface IBaseRequest<in TArgument, TResponse> : IBaseRequest
  {
    Task<TResponse> ExecuteAsync(TArgument arg);
  }
}
