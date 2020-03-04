using CommonModel.DeviceSettings;
using Newtonsoft.Json;
using System.Linq;
using CommonModels = CommonModel.DeviceSettings;

namespace Infrastructure.Service.DeviceMessageConstructor.Helpers
{
	public static class ConstructorHelpers
    {
        public static DeviceDetails GetDeviceConfigMsg(DeviceDetails deviceDetails, params string[] parameter)
        {
            string groupString = JsonConvert.SerializeObject(deviceDetails.Group);
            var deviceDtlsObj = JsonConvert.DeserializeObject<CommonModels.ParamGroup>(groupString);

            var devObj = new DeviceDetails()
            {
                Group = deviceDtlsObj
            };

            devObj.Group.Parameters = deviceDetails.Group.Parameters.Where(e => parameter.Contains(e.ParameterName)).ToList();

            return new DeviceDetails()
            {
                AssetUid = deviceDetails.AssetUid,
                DeviceUid = deviceDetails.DeviceUid,
                EventUtc = deviceDetails.EventUtc,
                DeviceType = deviceDetails.DeviceType,
                Group = devObj.Group,
                SerialNumber = deviceDetails.SerialNumber,
                UserUid = deviceDetails.UserUid
            };
        }
    }
}