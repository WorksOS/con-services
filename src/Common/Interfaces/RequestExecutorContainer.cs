using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Common.Interfaces
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    private const string ERROR_MESSAGE = "Failed to get/update data requested by {0}";
    private const string ERROR_MESSAGE_EX = "{0} with error: {1}";
    private const int ERROR_STATUS_OK = 0;

    /// <summary>
    /// Raptor client used in ProcessEx
    /// </summary>
    protected IASNodeClient raptorClient;

    /// <summary>
    /// Tag processor client interface used in ProcessEx
    /// </summary>
    protected ITagProcessor tagProcessor;

    /// <summary>
    /// Logger for logging
    /// </summary>
    protected ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore configStore;

    /// <summary>
    /// To talk to TCC with
    /// </summary>
    protected IFileRepository fileRepo;

    /// <summary>
    /// For handling DXF tiles
    /// </summary>
    protected ITileGenerator tileGenerator;

    /// <summary>
    /// For handling imported files
    /// </summary>
    protected List<FileData> fileList;

    /// <summary>
    /// For working with profiles
    /// </summary>
    protected ICompactionProfileResultHelper profileResultHelper;

    protected ITransferProxy transferProxy;

    protected ITRexTagFileProxy tRexTagFileProxy;

    protected ITRexCompactionDataProxy trexCompactionDataProxy;

    protected IDictionary<string, string> customHeaders;


    /// <summary>
    /// Gets the available contract execution error states.
    /// </summary>
    /// <value>
    /// The contract execution states.
    /// </value>
    protected ContractExecutionStatesEnum ContractExecutionStates { get; }

    /// <summary>
    /// Default constructor which creates all structures necessary for error handling.
    /// </summary>
    protected RequestExecutorContainer()
    {
      ContractExecutionStates = new ContractExecutionStatesEnum();
    }

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
    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    protected virtual Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Missing asynchronous executor process method override"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ServiceException"></exception>
    public ContractExecutionResult 
      Process<T>(T item)
    {
      ValidateTItem(item);
      return ProcessEx(item);
    }

    public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      ValidateTItem(item);
      return await ProcessAsyncEx(item);
    }

    private static void ValidateTItem<T>(T item)
    {
      if (item == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      }
    }

    /// <summary>
    /// Dynamically defines new error codes for the executor instance. Don't forget to clean them up after exit.
    /// </summary>
    protected virtual void ProcessErrorCodes()
    { }

    public void Initialise(ILogger logger, IASNodeClient raptorClient, ITagProcessor tagProcessor, 
      IConfigurationStore configStore, IFileRepository fileRepo, ITileGenerator tileGenerator, List<FileData> fileList, ICompactionProfileResultHelper profileResultHelper,
      ITransferProxy transferProxy, ITRexTagFileProxy tRexTagFileProxy, ITRexCompactionDataProxy trexCompactionDataProxy, IDictionary<string, string> customHeaders)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      log = logger;
      this.configStore = configStore;
      this.fileRepo = fileRepo;
      this.tileGenerator = tileGenerator;
      this.fileList = fileList;
      this.profileResultHelper = profileResultHelper;
      this.transferProxy = transferProxy;
      this.tRexTagFileProxy = tRexTagFileProxy;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
      this.customHeaders = customHeaders;
    }

    protected void ThrowRequestTypeCastException<T>(string errorMessage = null)
    {
      if (errorMessage == null)
        errorMessage = $"{nameof(T)} cast failed.";

      throw new ServiceException(
        HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, errorMessage));
    }

    protected ServiceException CreateServiceException<T>(int errorStatus = ERROR_STATUS_OK)
    {
      var errorMessage = string.Format(ERROR_MESSAGE, nameof(T));

      if (errorStatus > ERROR_STATUS_OK)
        errorMessage = string.Format(ERROR_MESSAGE_EX, ERROR_MESSAGE, ContractExecutionStates.FirstNameWithOffset(errorStatus));

      return new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, errorMessage));
    }

    /// <summary>
    /// Default destructor which destroys all structures necessary for error handling.
    /// </summary>
    ~RequestExecutorContainer()
    {
      ContractExecutionStates?.ClearDynamic();
    }
  }
}
