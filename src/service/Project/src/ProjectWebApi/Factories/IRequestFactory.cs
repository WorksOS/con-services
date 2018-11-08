using System;
using VSS.MasterData.Project.WebAPI.Common.Helpers;

namespace VSS.MasterData.Project.WebAPI.Factories
{
  /// <summary>
  /// Factory interface for creating <see cref="DataRequestBase"/> instances.
  /// </summary>
  public interface IRequestFactory
  {
    /// <summary>
    /// Instantiates the object of type T.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="DataRequestBase"/></typeparam>
    /// <param name="action">Action delegate to setup the new <see cref="DataRequestBase"/> instance.</param>
    /// <returns>Returns instance of T with required attributes set.</returns>
    T Create<T>(Action<RequestFactory> action) where T : DataRequestBase, new();

    /// <summary>
    /// Sets the customerUid from the authentication header.
    /// </summary>
    /// <param name="customerUid">Customer Uid provided by the request header.</param>
    /// <returns>Returns this instance of <see cref="RequestFactory"/>.</returns>
    RequestFactory CustomerUid(string customerUid);
  }
}