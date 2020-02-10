using CommonModel.Error;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig
{
    public class DeviceConfigServiceResponse<T>
    {
        public DeviceConfigServiceResponse(IList<T> lists, IList<IErrorInfo> errorsInfo = null)
        {
            this.Lists = lists;
            this.Errors = errorsInfo;
        }
        public IList<T> Lists { get; set; }
        public IList<IErrorInfo> Errors { get; set; }
    }
}
