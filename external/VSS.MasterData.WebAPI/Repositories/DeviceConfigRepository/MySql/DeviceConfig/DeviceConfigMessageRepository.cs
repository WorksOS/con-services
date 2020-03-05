//TODO: Uncomment and compile, Only when needed
//using DbModel.DeviceConfig;
//using Interfaces;
//using System;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using VSS.MasterData.WebAPI.Transactions;

//namespace DeviceConfigRepository.MySql.DeviceConfig
//{
//	public class DeviceConfigMessageRepository : IDeviceConfigMessageRepository
//    {
//        private readonly ILoggingService _loggingService;
//		private readonly ITransactions _transactions;

//		public DeviceConfigMessageRepository(ITransactions transactions, ILoggingService loggingService)
//        {
//            this._transactions = transactions;
//            this._loggingService = loggingService;
//            this._loggingService.CreateLogger(this.GetType());
//        }

//        public async Task<DeviceConfigMessageDto> Fetch(string messageUID)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DeviceConfigMessageRepository.Fetch");
//                var response = await this._transactions.GetAsync<DeviceConfigMessageDto>(string.Format(Queries.SELECT_DEVICECONFIGMESSAGE, messageUID));
//                return response.FirstOrDefault();
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }

//        public async Task<bool> Update(string messageUID)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DeviceConfigMessageRepository.Update");
//                var response = await this._transactions.ExecuteAsync(string.Format(Queries.UPDATE_DEVICECONFIGMESSAGE_STATUS, messageUID));
//                return response > 0;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }

//        // For ComponentTest
//        public async Task<bool> Insert(DeviceConfigMessageDto deviceConfigMessageDto)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DeviceConfigMessageRepository.Insert");
//                var response = await this._transactions.ExecuteAsync(string.Format(Queries.INSERT_DEVICECONFIGMESSAGE, deviceConfigMessageDto.DeviceConfigMessageUIDString, deviceConfigMessageDto.DeviceUIDString), new
//                {
//                    fk_DeviceTypeID = deviceConfigMessageDto.DeviceTypeID,
//                    EventUTC = deviceConfigMessageDto.EventUTC,
//                    MessageContent = deviceConfigMessageDto.MessageContent,
//                    fk_StatusID = deviceConfigMessageDto.StatusID,
//                    LastMessageUTC = deviceConfigMessageDto.LastMessageUTC
//                });
//                return response > 0;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }
//    }
//}
