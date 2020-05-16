using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.Common.Interfaces
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from controller logic for testability and possible executor versioning.%
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    protected bool UseTRexGateway(string key) => configStore.GetValueBool(key) ?? false;
    protected bool UseRaptorGateway(string key) => configStore.GetValueBool(key) ?? false;
    protected bool IsTRexAvailable(string key) => configStore.GetValueBool(key) ?? false;

    private const string ERROR_MESSAGE = "Failed to get/update data requested by {0}";
    private const string ERROR_MESSAGE_EX = "{0} with error: {1}";
    private const int ERROR_STATUS_OK = 0;

#if RAPTOR
    /// <summary>
    /// Raptor client used in ProcessEx
    /// </summary>
    protected IASNodeClient raptorClient;

    /// <summary>
    /// Tag processor client interface used in ProcessEx
    /// </summary>
    protected ITagProcessor tagProcessor;
#endif

    /// <summary>
    /// Logger for logging
    /// </summary>
    protected ILogger log;
    protected ILoggerFactory loggerFactory;

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

    protected ITRexConnectedSiteProxy tRexConnectedSiteProxy;

    protected ITRexCompactionDataProxy trexCompactionDataProxy;

    protected IDeviceProxy deviceProxy;

    protected IHeaderDictionary customHeaders;

    protected string customerUid;


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
    protected virtual ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Missing synchronous executor process method override or Use the asynchronous form of this method");
    }

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

    public void Initialise(ILoggerFactory loggerFactory, ILogger logger,
#if RAPTOR
      IASNodeClient raptorClient,
      ITagProcessor tagProcessor,
#endif
      IConfigurationStore configStore, IFileRepository fileRepo, ITileGenerator tileGenerator, List<FileData> fileList, ICompactionProfileResultHelper profileResultHelper,
      ITransferProxy transferProxy, ITRexTagFileProxy tRexTagFileProxy, ITRexConnectedSiteProxy tRexConnectedSiteProxy, ITRexCompactionDataProxy trexCompactionDataProxy,
      IDeviceProxy deviceProxy, IHeaderDictionary customHeaders, string customerUid)
    {
      this.loggerFactory = loggerFactory;
      this.log = logger;
#if RAPTOR
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
#endif
      this.configStore = configStore;
      this.fileRepo = fileRepo;
      this.tileGenerator = tileGenerator;
      this.fileList = fileList;
      this.profileResultHelper = profileResultHelper;
      this.transferProxy = transferProxy;
      this.tRexTagFileProxy = tRexTagFileProxy;
      this.tRexConnectedSiteProxy = tRexConnectedSiteProxy;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
      this.deviceProxy = deviceProxy;
      this.customHeaders = customHeaders;
      this.customerUid = customerUid;
    }
    
    /// <summary>
    /// Casts input object to type T for use with child executors.
    /// </summary>
    protected T CastRequestObjectTo<T>(object item) where T : ProjectID
    {
      var request = item as T;

      if (request == null)
      {
        ThrowRequestTypeCastException<T>();
      }

      return request;
    }

    /// <summary>
    /// Casts input object to type ProjectIDs for use with child executors.
    /// </summary>
    protected ProjectIDs CastRequestObjectToProjectIDs(object item)
    {
      var request = item as ProjectIDs;

      if (request == null)
      {
        ThrowRequestTypeCastException<ProjectIDs>();
      }

      return request;
    }

    protected T CastTrexRequestObjectTo<T>(object item) where T : TRexBaseRequest
    {
      var request = item as T;

      if (request == null)
      {
        ThrowRequestTypeCastException<T>();
      }

      return request;
    }

    protected void ThrowRequestTypeCastException<T>(string errorMessage = null)
    {
      if (errorMessage == null)
        errorMessage = $"{typeof(T).Name} cast failed.";

      throw new ServiceException(
        HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, errorMessage));
    }

    protected ServiceException CreateServiceException<T>(int errorStatus = ERROR_STATUS_OK)
    {
      var errorMessage = string.Format(ERROR_MESSAGE, typeof(T).Name);

      if (errorStatus > ERROR_STATUS_OK)
        errorMessage = string.Format(ERROR_MESSAGE_EX, errorMessage, ContractExecutionStates.FirstNameWithOffset(errorStatus));

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
