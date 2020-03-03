using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Response.DeviceConfig.Ping;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DevicePingService : DeviceConfigServiceBase, IDevicePingService
	{

		private readonly IEnumerable<IRequestValidator<IServiceRequest>> _validators;
		private readonly IMessageConstructor _messageConstructor;
		private readonly IDevicePingRepository _devicePingRepository;
		private readonly IRequestValidator<DevicePingLogRequest> _pingValidator;

		public DevicePingService(IDevicePingRepository devicePingRepository, ILoggingService loggingService, IMessageConstructor messageConstructor,
			IEnumerable<IRequestValidator<IServiceRequest>> validators, IRequestValidator<DevicePingLogRequest> pingValidator, ITransactions transactions, IMapper mapper)
			: base(null, mapper, loggingService)
		{
			_devicePingRepository = devicePingRepository;
			_messageConstructor = messageConstructor;
			_validators = validators;
			_pingValidator = pingValidator;
		}

		public async Task<DevicePingStatusResponse> GetPingRequestStatus(DevicePingLogRequest request)
		{
			PingRequestStatus pingRequestStatus = new PingRequestStatus();
			DevicePingStatusResponse response;
			List<IErrorInfo> errorInfos = new List<IErrorInfo>();

			errorInfos.AddRange(await base.Validate(this._validators, request));
			errorInfos.AddRange(await this._pingValidator.Validate(request));

			if (errorInfos.Count <= 0)
			{
				this._loggingService.Info("Started Invoking DevicePingRepository with request : " + JsonConvert.SerializeObject(request), "DevicePingService.Fetch");

				pingRequestStatus = await this._devicePingRepository.Fetch(request);
				if (pingRequestStatus != null)
				{
					RequestStatus rs = (RequestStatus)pingRequestStatus.RequestStatusID;
					pingRequestStatus.RequestState = rs.ToString();
				}
				this._loggingService.Info("Ended Invoking DevicePingRepository with response : " + JsonConvert.SerializeObject(pingRequestStatus), "DevicePingService.Fetch");
				response = _mapper.Map<DevicePingStatusResponse>(pingRequestStatus);
			}
			else
			{
				response = new DevicePingStatusResponse
				{
					AssetUID = request.AssetUID.ToString(),
					DeviceUID = request.DeviceUID.ToString(),
					Errors = errorInfos
				};
			}

			return response;
		}

		public async Task<DevicePingStatusResponse> PostDevicePingRequest(DevicePingLogRequest request)
		{
			PingRequestStatus pingRequestStatus = new PingRequestStatus();
			DevicePingStatusResponse response;
			DevicePingLogRequest validatedRequest = new DevicePingLogRequest();
			Guid devicePingLogUID = Guid.Empty;
			List<IErrorInfo> errorInfos = new List<IErrorInfo>();
			response = new DevicePingStatusResponse
			{
				AssetUID = request.AssetUID.ToString(),
				DeviceUID = request.DeviceUID.ToString(),
			};
			errorInfos.AddRange(await base.Validate(this._validators, request));
			errorInfos.AddRange(await this._pingValidator.Validate(request));

			//errorInfos.AddRange(await this.ValidateDuplicateRequest(request));

			var latestRequest = await _devicePingRepository.Fetch(request);

			if (latestRequest != null)
			{
				if (((latestRequest.RequestStatusID == (int)RequestStatus.Pending) || (latestRequest.RequestStatusID == (int)RequestStatus.Acknowledged)) && DateTime.UtcNow <= latestRequest.RequestExpiryTimeUTC)
				{
					errorInfos.AddRange(new List<IErrorInfo>() { (new ErrorInfo() { Message = Utils.GetEnumDescription(ErrorCodes.DuplicatePingRequest), ErrorCode = (int)ErrorCodes.DuplicatePingRequest }) });
					response.AssetUID = latestRequest.AssetUID.ToString();
					response.DeviceUID = latestRequest.DeviceUID.ToString();
					response.DevicePingLogUID = latestRequest.DevicePingLogUID.ToString();
					response.RequestExpiryTimeUTC = latestRequest.RequestExpiryTimeUTC;
					response.RequestStatusID = latestRequest.RequestStatusID;
					RequestStatus rs = (RequestStatus)latestRequest.RequestStatusID;
					response.RequestState = rs.ToString();
					response.RequestTimeUTC = latestRequest.RequestTimeUTC;
				}
			}

			if (errorInfos.Count <= 0)
			{
				try
				{
					DeviceTypeFamily deviceTypeFamily = await _devicePingRepository.GetDeviceTypeFamily(request.DeviceUID);
					if (deviceTypeFamily == null)
					{
						errorInfos.Add(new ErrorInfo
						{
							ErrorCode = (int)ErrorCodes.DeviceTypeFamilyNotSupported,
							Message = Utils.GetEnumDescription(ErrorCodes.DeviceTypeFamilyNotSupported),
							IsInvalid = true,
						});
						response.Errors = errorInfos;
						return response;
					}
					request.FamilyName = deviceTypeFamily.FamilyName;
					devicePingLogUID = _messageConstructor.ProcessMessage(request.AssetUID, request.DeviceUID, deviceTypeFamily.FamilyName);
					if (devicePingLogUID == Guid.Empty)
					{
						errorInfos.Add(new ErrorInfo
						{
							ErrorCode = (int)ErrorCodes.UnexpectedError,
							Message = Utils.GetEnumDescription(ErrorCodes.UnexpectedError),
							IsInvalid = true,
						});
						response.Errors = errorInfos;
						return response;
					}
					request.DevicePingLogUID = devicePingLogUID;
					this._loggingService.Info("Started Invoking DevicePingRepository with request : " + JsonConvert.SerializeObject(request), "DevicePingService.Insert");
					pingRequestStatus = await this._devicePingRepository.Insert(request);
					this._loggingService.Info("Ended Invoking DevicePingRepository with response : " + JsonConvert.SerializeObject(pingRequestStatus), "DevicePingService.Insert");
					response = _mapper.Map<DevicePingStatusResponse>(pingRequestStatus);
				}
				catch (Exception ex)
				{
					response.Errors.Add(new ErrorInfo
					{
						ErrorCode = (int)ErrorCodes.UnexpectedError,
						Message = Utils.GetEnumDescription(ErrorCodes.UnexpectedError),
						IsInvalid = true,
					});
					this._loggingService.Error("Exception occurred while saving Ping Request", "PingController.PostDevicePingRequest", ex);
					throw ex;
				}
			}
			else
			{
				response.Errors = errorInfos;
			}

			return response;
		}
		private async Task<IList<IErrorInfo>> ValidateDuplicateRequest(DevicePingLogRequest devicePingLogRequest)
		{
			var latestRequest = await _devicePingRepository.Fetch(devicePingLogRequest);
			IList<IErrorInfo> errors = new List<IErrorInfo>();
			if (latestRequest != null)
			{
				if (((latestRequest.RequestStatusID == (int)RequestStatus.Pending) || (latestRequest.RequestStatusID == (int)RequestStatus.Acknowledged)) && DateTime.UtcNow <= latestRequest.RequestExpiryTimeUTC)
				{
					errors.Add(new ErrorInfo() { Message = Utils.GetEnumDescription(ErrorCodes.DuplicatePingRequest), ErrorCode = (int)ErrorCodes.DuplicatePingRequest });
				}
			}
			return errors;
		}
		private async Task<DevicePingLogRequest> ConstructMessage(DevicePingLogRequest devicePingLogRequest)
		{

			bool result = false;
			try
			{
				return devicePingLogRequest;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
