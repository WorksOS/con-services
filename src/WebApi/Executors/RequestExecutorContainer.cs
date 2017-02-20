using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApi.Interfaces
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer //: ICustomErrors
  {
    /// <summary>
    /// Repository factory used in ProcessEx
    /// </summary>
    protected IRepositoryFactory factory;

    /// <summary>
    ///   Generates the errorlist for instantiated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      return (from object enumVal in Enum.GetValues(typeof(ContractExecutionStatesEnum))
              select new Tuple<int, string>((int)enumVal, enumVal.ToString())).ToList();
    }


    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    /// <param name="guidsWithDates">Results from applying subscription logic to the request</param>
    /// <returns></returns>
    protected abstract ContractExecutionResult ProcessEx<T>(T item); // where T : IServiceDomainObject;

    internal static object Build<T>()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return ProcessEx(item);
    }

    /// <summary>
    ///   Builds this instance for specified executor type.
    /// </summary>
    /// <param name="factory">Repository factory</param>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(IRepositoryFactory factory) where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() { factory = factory };
      return executor;
    }
  }
}