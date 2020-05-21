using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;

namespace VSS.Productivity3D.TagFileGateway.Common.Executors
{
    public abstract class RequestExecutorContainer
    {
        private ILogger _logger;
        protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

        protected ILoggerFactory LoggerFactory { get; private set; }
        
        protected IConfigurationStore ConfigStore { get; private set; }
        protected IDataCache DataCache { get; private set; }
        protected ITagFileForwarder TagFileForwarder { get; private set; }
        protected ITransferProxy TransferProxy { get; private set; }
        protected IWebRequest WebRequest { get; private set; }
        protected abstract Task<ContractExecutionResult> ProcessAsyncEx<T>(T item);

        /// <summary> </summary>
        public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
        {
            if (item == null)
                throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
            return await ProcessAsyncEx(item);
        }

        public static TExecutor Build<TExecutor>(ILoggerFactory loggerFactory,
          IConfigurationStore configStore,
          IDataCache dataCache,
          ITagFileForwarder tagFileForwarder,
          ITransferProxy transferProxy,
          IWebRequest webRequest)
          where TExecutor : RequestExecutorContainer, new()
        {
            var executor = new TExecutor()
            {
                LoggerFactory = loggerFactory,
                ConfigStore = configStore,
                DataCache = dataCache,
                TagFileForwarder = tagFileForwarder,
                TransferProxy = transferProxy,
                WebRequest = webRequest
            };
            return executor;
        }

        public TExecutor Build<TExecutor>()
          where TExecutor : RequestExecutorContainer, new()
        {
          var executor = new TExecutor()
          {
            LoggerFactory = LoggerFactory,
            ConfigStore = ConfigStore,
            DataCache = DataCache,
            TagFileForwarder = TagFileForwarder,
            TransferProxy = TransferProxy,
            WebRequest = WebRequest
          };
          return executor;
        }
    }
}
