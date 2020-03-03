//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using Interfaces;
//using Infrastructure.Common.DeviceSettings.Enums;

//namespace DeviceConfigRepository.MySql
//{
//    public class DevicePingStateChangerService : IDevicePingStateChangerService
//    {
//        private readonly IConnection _connection;
//        private readonly ILoggingService _loggingService;
//        public DevicePingStateChangerService(IConnection connection, ILoggingService loggingService)
//        {
//            this._connection = connection;
//            this._loggingService = loggingService;
//            this._loggingService.CreateLogger(this.GetType());
//        }

//        public async Task<DateTime> GetAcknowledgeTimeUTCFromDeviceACKMessageForValidPing(string assetUID, string deviceUID, DateTime requestTime)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "DevicePingStateChangerService.GetAcknowledgeTimeUTCFromDeviceACKMessageForValidPing");
//                string acknowledgeTimeQuery = @"Select Max(DPA.AcknowledgeTimeUTC) From DevicePingLog DPL 
//                                                Inner Join DevicePingACKMessage DPA On DPL.DevicePingLogUID = DPA.fk_DevicePingLogUID
//                                                where 
//                                                DPL.RequestExpiryTimeUTC > @RequestTime and 
//                                                DPA.fk_DeviceUID = {0} and 
//                                                DPA.fk_AssetUID = {1} and 
//                                                DPA.fk_AcknowledgeStatusID = {2} and 
//                                                DPL.RequestTimeUTC <= @RequestTime 
//                                                Group By DPA.fk_DevicePingLogUID;";
//                var query = string.Format(acknowledgeTimeQuery,
//                    string.Format("0x{0}", Guid.Parse(deviceUID).ToString("N")),
//                    string.Format("0x{0}", Guid.Parse(assetUID).ToString("N")),
//                    (int)RequestStatus.Acknowledged);
//                _loggingService.Debug(string.Format("Query Built : {0}", query), "DevicePingStateChangerService.GetAcknowledgeTimeUTCFromDeviceACKMessageForValidPing");
//                var response = await this._connection.FetchAsync<DateTime>(query, new { RequestTime = requestTime } );
//                this._loggingService.Debug(string.Format("Ended executing query Result : {0}", response.Count()), "DevicePingStateChangerService.GetAcknowledgeTimeUTCFromDeviceACKMessageForValidPing");
//                return response.Count() > 0 ? response.First() : DateTime.MinValue;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", "LoginUserDto.Insert", ex);
//                throw ex;
//            }
//        }

//        public async Task<bool> UpsertPing(string assetUID, string deviceUID, DateTime eventUTC)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "LoginUserDto.Insert");
//                string upsertDeviceACKMessage = @"Update DevicePingLog
//	                                                    Set 
//                                                            fk_RequestStatusID = {3}, 
//                                                            ResponseTimeUTC = @RequestTime, 
//                                                            RowUpdatedUTC = utc_timestamp()
//                                                    Where fk_AssetUID = {0} and fk_DeviceUID = {1} 
//                                                    and RequestExpiryTimeUTC > @RequestTime and fk_RequestStatusID = {2} 
//                                                    and RequestTimeUTC <= @RequestTime;";
//                var response = await this._connection.ExecuteAsync(string.Format(upsertDeviceACKMessage, 
//                                                                    string.Format("0x{0}", Guid.Parse(assetUID).ToString("N")), 
//                                                                    string.Format("0x{0}", Guid.Parse(deviceUID).ToString("N")),
//                                                                    (int)RequestStatus.Acknowledged,
//                                                                    (int)RequestStatus.Completed), new { RequestTime = eventUTC });
//                this._loggingService.Debug("Ended executing query", "DevicePingStateChangerService.InsertPing");
//                return response > 0;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", "LoginUserDto.Insert", ex);
//                throw ex;
//            }
//        }
//    }
//}
