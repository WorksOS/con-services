using System.Threading.Tasks;

namespace VSS.TRex.GridFabric.Interfaces
{
  /// <summary>
  /// Provides a generic interface that can represent an Ignite request in terms of its argument and results
  /// </summary>
  /// <typeparam name="TArgument"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  public interface IBaseRequest<in TArgument, TResponse>
  {
    TResponse Execute(TArgument arg);
    Task<TResponse> ExecuteAsync(TArgument arg);
  }
}
