using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;
using TCCFileAccess;

namespace VSS.Raptor.Service.Common.Interfaces
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer 
  {
    /// <summary>
    /// Raptor client used in ProcessEx
    /// </summary>
    protected IASNodeClient raptorClient;

    /// <summary>
    /// Tag processor client interface used in ProcessEx
    /// </summary>
    protected ITagProcessor tagProcessor { get; set; }

    /// <summary>
    /// Logger for logging
    /// </summary>
    protected ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore configStore;

    /// <summary>
    /// Service provider
    /// </summary>
    protected IFileRepository fileAccess;


    /// <summary>
    /// Generates the dynamic errorlist for instanciated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      List<Tuple<int, string>> result = new List<Tuple<int, string>>();
      for (int i = 0; i < ContractExecutionStates.Count; i++)
      {
        result.Add(new Tuple<int, string>(ContractExecutionStates.ValueAt(i),
               ContractExecutionStates.NameAt(i)));
      }
      ContractExecutionStates.ClearDynamic();
      return result;
    }

    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    /// <returns></returns>
    protected abstract ContractExecutionResult ProcessEx<T>(T item); 

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
    /// Gets the available contract execution error states.
    /// </summary>
    /// <value>
    /// The contract execution states.
    /// </value>
    protected ContractExecutionStatesEnum ContractExecutionStates { get; private set; }

    /// <summary>
    /// Dynamically defines new error codes for the executor instance. Don't forget to clean them up after exit.
    /// </summary>
    protected virtual void ProcessErrorCodes()
    {
    }

    /// <summary>
    /// Injected constructor for mocking.
    /// </summary>
    protected RequestExecutorContainer(ILoggerFactory logger, IASNodeClient raptorClient) : this()
    {
      this.raptorClient = raptorClient;
      if (logger != null)
        this.log = logger.CreateLogger<RequestExecutorContainer>();
    }

    /// <summary>
    /// Injected constructor for mocking.
    /// </summary>
    protected RequestExecutorContainer(ILoggerFactory logger, IASNodeClient raptorClient, ITagProcessor tagProcessor) : this()
    {
        this.raptorClient = raptorClient;
        this.tagProcessor = tagProcessor;
        if (logger != null)
            this.log = logger.CreateLogger<RequestExecutorContainer>();
    }

    /// <summary>
    /// Injected constructor for mocking.
    /// </summary>
    protected RequestExecutorContainer(ILoggerFactory logger, IASNodeClient raptorClient, ITagProcessor tagProcessor, IConfigurationStore configStore) : this()
    {
        this.raptorClient = raptorClient;
        this.tagProcessor = tagProcessor;
        if (logger != null)
            this.log = logger.CreateLogger<RequestExecutorContainer>();
        this.configStore = configStore;
    }

    protected RequestExecutorContainer(ILoggerFactory logger, IASNodeClient raptorClient, ITagProcessor tagProcessor, IConfigurationStore configStore, IFileRepository fileAccess) : this()
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      if (logger != null)
        this.log = logger.CreateLogger<RequestExecutorContainer>();
      this.configStore = configStore;
      this.fileAccess = fileAccess;
    }

    /// <summary>
    /// Default constructor which creates all structures necessary for error handling.
    /// </summary>
    protected RequestExecutorContainer()
    {
      ContractExecutionStates = new ContractExecutionStatesEnum();
      ProcessErrorCodes();
    }

    //TODO: Check if this works
    /// <summary>
    /// Default destructor which destroys all structures necessary for error handling.
    /// </summary>
    ~RequestExecutorContainer()
    {
      if (ContractExecutionStates != null)
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    /// <summary>
    ///   Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(ILoggerFactory logger, IASNodeClient raptorClient, ITagProcessor tagProcessor=null, IConfigurationStore configStore=null, IFileRepository fileAccess=null) 
      where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() {raptorClient = raptorClient, tagProcessor = tagProcessor, log = logger.CreateLogger<TExecutor>(), configStore = configStore, fileAccess = fileAccess};
      return executor;
    }

  }
}
