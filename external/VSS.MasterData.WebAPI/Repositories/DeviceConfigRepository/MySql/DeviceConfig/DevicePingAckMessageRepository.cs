//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using Interfaces;
//using DbModel;
//using DbModel.DeviceConfig;
//using Infrastructure.Common.DeviceSettings.Enums;
//using Infrastructure.Common.DeviceSettings.Helpers;
//using VSS.MasterData.WebAPI.Transactions;

//namespace DeviceConfigRepository.MySql.DeviceConfig
//{
//    public class DevicePingAckMessageRepository : IDevicePingAckMessageRepository
//    {
//        private readonly ITransactions _transactions;
//        private readonly ILoggingService _loggingService;


//        public DevicePingAckMessageRepository(ITransactions transactions, ILoggingService loggingService)
//        {
//            this._transactions = transactions;
//            this._loggingService = loggingService;
//            this._loggingService.CreateLogger(this.GetType());
//        }

//        public async Task<DevicePingACKMessageDto> Fetch(string messageUID)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DevicePingAckMessageRepository.Fetch");
//                var response = await this._transactions.GetAsync<DevicePingACKMessageDto>(string.Format(Queries.SELECT_DEVICEPINGACKMESSAGE, messageUID));
//                return response.FirstOrDefault();
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }

//        public async Task<bool> Update(DevicePingACKMessageDto devicePingACKMessageDto)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DevicePingAckMessageRepository.Update");
//                var response = await this._transactions.Upsert(new DevicePingACKMessageDto
//				{
//                    AcknowledgeStatusID = RequestStatus.Acknowledged,
//                    AcknowledgeTimeUTC = devicePingACKMessageDto.AcknowledgeTimeUTC,
//                    RowUpdatedUTC = DateTime.UtcNow,
//                    FilterStatus = RequestStatus.Pending
//                });
//                return response > 0;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }

//        public async Task<bool> UpdateRequestStatusInDevicePingLog(DevicePingACKMessageDto devicePingACKMessageDto)
//        {
//            try
//            {
//                var response = await this._transactions.Upsert(string.Format(Queries.UPDATE_DEVICEPINGLOG_REQUESTSTATUS, devicePingACKMessageDto.DevicePingLogUID.ToString(), (int)RequestStatus.Pending), new
//                {
//                    RequestStatusID = (int)RequestStatus.Acknowledged,
//                    RowUpdatedUTC = DateTime.UtcNow,
//                });
//                return response > 0;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }

//        public async Task<IEnumerable<PingRequestStatus>> FetchDevicePingLogs(DevicePingACKMessageDto devicePingACKMessageDto)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DevicePingAckMessageRepository.Fetch");
//                var response = await this._transactions.GetAsync<PingRequestStatus>(string.Format(Queries.SELECT_DEVICEPINGLOGBYMESSAGEID, devicePingACKMessageDto.DevicePingLogUID.ToStringWithoutHyphens(), devicePingACKMessageDto.DevicePingACKMessageUID.ToStringWithoutHyphens()), new { PendingStatusID = (int)RequestStatus.Pending });
//                return response;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
//                throw ex;
//            }
//        }
//    }
//}
